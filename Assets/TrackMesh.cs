using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrackMesh : MonoBehaviour
{
    [SerializeField] private TrackMaker track;
    [SerializeField] private float trackWidth = 1f;
    private Mesh mesh;
    private MeshFilter meshFilter;

    private void Awake()
    {
        if (track == null)
        {
            track = GetComponent<TrackMaker>();
        }
        meshFilter = GetComponent<MeshFilter>();

        transform.position = Vector2.zero;
    }

    private void Start()
    {
        transform.position = track.transform.position;
    }

    private void UpdateMesh()
    {
        if (mesh != null)
        {
            mesh.Clear();
            Destroy(mesh);
            mesh = null;
        }

        List<Vector2> points = track.GetListOfPoints();
        Vector2 point = points[0];
        Vector2 secondPoint = points[1];
        mesh = MeshUtils.CreateLineMesh((Vector3)point - transform.position, (Vector3)secondPoint - transform.position, Vector3.back, trackWidth);

        for (int i = 2; i < points.Count+2; i++)
        {
            Vector2 thisPoint = points[i%points.Count];

            MeshUtils.AddLinePoint(mesh, (Vector3)thisPoint - transform.position, Vector3.back, trackWidth);
        }

        meshFilter.mesh = mesh;
    }

    private void OnEnable()
    {
        track.MapChange += UpdateMesh;
    }
    private void OnDisable()
    {
        track.MapChange -= UpdateMesh;
    }
}
