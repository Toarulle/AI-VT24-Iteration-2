using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrackMaker : MonoBehaviour
{
    [SerializeField] private GameObject ground;
    [SerializeField] private bool showGizmos = true;
    public int dotMiddle = 15;
    public int dotSpread = 5;
    public int pushIterations = 5;

    private float height = 10;
    private float width = 10;
    List<Vector2> points = new List<Vector2>();
    List<Vector2> hull = new List<Vector2>();
    
    void Start()
    {
        height = ground.transform.localScale.y;
        width = ground.transform.localScale.x;

        NewMap();
    }

    [ContextMenu("New Map")]
    public void NewMap()
    {
        points = new List<Vector2>();
        hull = new List<Vector2>();
        int pointCount = Random.Range(dotMiddle - dotSpread, dotMiddle + dotSpread);

        for (int i = 0; i < pointCount; i++)
        {
            float x = Random.Range(0f, width) - width / 2;
            float y = Random.Range(0f, height) - height / 2;
            points.Add(new Vector2(x,y));
        }

        hull = ConvexHull(points, pointCount);

        for (int i = 0; i < pushIterations; i++)
        {
            PushApart(hull);
        }
    }

    private void PushApart(List<Vector2> dataSet)
    {
        float dist = 15;
        float dist2 = dist * dist;

        for (int i = 0; i < dataSet.Count; ++i)
        {
            for (int j = 0; j < dataSet.Count; ++j)
            {
                if (Vector2.Distance(dataSet[i],dataSet[j]) < dist2)
                {
                    float deltaX = dataSet[j].x - dataSet[i].x;
                    float deltaY = dataSet[j].y - dataSet[i].y;
                    float deltaMag = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    deltaX /= deltaMag;
                    deltaY /= deltaMag;

                    float diff = dist - deltaMag;
                    deltaX *= diff;
                    deltaY *= diff;
                    Vector2 newJ = dataSet[j];
                    newJ.x += deltaX;
                    newJ.y += deltaY;
                    dataSet[j] = newJ;
                    Vector2 newI = dataSet[j];
                    newI.x -= deltaX;
                    newI.y -= deltaY;
                    dataSet[i] = newI;
                }
            }
        }
    }
    
    private List<Vector2> ConvexHull(List<Vector2> points, int length)
    {
        List<Vector2> hull = new List<Vector2>();

        int l = 0;
        for (int i = 0; i < length; i++)
        {
            if (points[i].x < points[l].x)
            {
                l = i;
            }
        }

        int p = 1;
        int q = 0;
        while (true)
        {
            hull.Add(points[p]);

            q = (p + 1) % length;

            for (int i = 0; i < length; i++)
            {
                if (Orientation(points[p],points[i],points[q]) == 2)
                {
                    q = i;
                }
            }

            p = q;

            if (p == l)
            {
                break;
            }
        }

        return hull;
    }
    
    private int Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float value = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

        if (value == 0)
            return 0;
        else if (value > 0)
            return 1;
        else
            return 2;
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;
        
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i],1f);
        }

        for (int i = 0; i < hull.Count; i++)
        {
            int next = i + 1;
            if (next == hull.Count)
            {
                next = 0;
            }
            Gizmos.DrawLine(hull[i], hull[next]);
        }
    }
}