using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TrackMaker : MonoBehaviour
{
    [Header("Basesettings")]
    [SerializeField] private int seed;
    [SerializeField] private GameObject ground;
    [SerializeField] private float groundWidth;
    [SerializeField] private float groundHeight;
    [SerializeField] private int groundWidthOffset;
    [SerializeField] private int groundHeightOffset;
    [Header("Dotspreading")]
    [SerializeField] private int dotAmountMiddle = 15;
    [SerializeField] private int dotAmountSpread = 5;
    [Header("Displacement")]
    [Range(0.02f,20f)] [SerializeField] private float difficulty = 1f;
    [SerializeField] private float maxDisplacement = 1f;
    [Header("Push apart")]
    [SerializeField] private int pushIterations = 5;
    [SerializeField] private int pushSmallestDistance = 5;
    [Header("Fix angles")]
    [SerializeField] private int largestTurnDegrees = 100;
    [SerializeField] private int fixAngleIterations = 10;
    [Header("CatmullRom")]
    [SerializeField] private int smoothingSteps = 5;
    [SerializeField] private int subtractIfClose = 4;
    [Header("Gizmos")] 
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool drawPoints, drawHull, drawSmoothHull, drawPath;
    [Header("Gridsettings")]
    [SerializeField] private GameObject startgrid;
    [Header("Car")]
    [SerializeField] private GameObject racecar;
    [SerializeField] private float carDistanceToGrid;

    public UnityAction MapChange = delegate {};
    
    private List<Vector2> points, hull, smoothHull;
    private int currentStartgridIndex = 0;
    public int Seed
    {
        get => seed;
        set => seed = value;
    }

    public float GroundWidth
    {
        get => groundWidth;
        set => groundWidth = value;
    }

    public float GroundHeight
    {
        get => groundHeight;
        set => groundHeight = value;
    }

    public float GroundHeightOffset
    {
        get => groundHeightOffset;
        set => groundHeightOffset = (int)value;
    }

    public float GroundWidthOffset
    {
        get => groundWidthOffset;
        set => groundWidthOffset = (int)value;
    }

    public float DotAmountMiddle
    {
        get => dotAmountMiddle;
        set => dotAmountMiddle = (int)value;
    }

    public float DotAmountSpread
    {
        get => dotAmountSpread;
        set => dotAmountSpread = (int)value;
    }

    public float Difficulty
    {
        get => difficulty;
        set => difficulty = (int)value;
    }

    public float MaxDisplacement
    {
        get => maxDisplacement;
        set => maxDisplacement = (int)value;
    }

    public float PushIterations
    {
        get => pushIterations;
        set => pushIterations = (int)value;
    }

    public float PushSmallestDistance
    {
        get => pushSmallestDistance;
        set => pushSmallestDistance = (int)value;
    }

    public float LargestTurnDegrees
    {
        get => largestTurnDegrees;
        set => largestTurnDegrees = (int)value;
    }

    public float FixAngleIterations
    {
        get => fixAngleIterations;
        set => fixAngleIterations = (int)value;
    }

    public float SmoothingSteps
    {
        get => smoothingSteps;
        set => smoothingSteps = (int)value;
    }

    public float SubtractIfClose
    {
        get => subtractIfClose;
        set => subtractIfClose = (int)value;
    }

    private void Awake()
    {
        groundHeight = ground.transform.localScale.y/2;
        groundWidth = ground.transform.localScale.x/2;
    }

    void Start()
    {
        if (seed == 0)
        {
            NewMapNewSeed();
        }
        else
        {
            NewMap();
        }
    }

    public List<Vector2> GetListOfPoints()
    {
        return smoothHull;
    }
    
    [ContextMenu("New Map - New seed")]
    public void NewMapNewSeed()
    {
        seed = Random.Range(1000000, 9999999);
        NewMap();
    }
    
    [ContextMenu("New Map - Current seed")]
    public void NewMap()
    {
        var newScale = new Vector3(groundWidth * 2, groundHeight * 2, 1);
        ground.transform.localScale = newScale;
        points = new List<Vector2>();
        hull = new List<Vector2>();
        Random.InitState(seed);
        int pointCount = Random.Range(dotAmountMiddle - dotAmountSpread, dotAmountMiddle + dotAmountSpread);
        for (int i = 0; i < pointCount; i++)
        {
            float x = Random.Range(-groundWidth+groundWidthOffset, groundWidth-groundWidthOffset);
            float y = Random.Range(-groundHeight+groundHeightOffset, groundHeight-groundHeightOffset);
            points.Add(new Vector2(x,y));
        }

        hull = ConvexHull(points, pointCount);

        for (int i = 0; i < pushIterations; i++)
        {
            PushApart(ref hull);
        }
    
        CurvesAndDisplacement(ref hull);
        for (int i = 0; i < pushIterations; i++)
        {
            PushApart(ref hull);
        }
        for (int i = 0; i < fixAngleIterations; i++)
        {
            CheckAngles(ref hull);
            PushApart(ref hull);
        }
        smoothHull = CatmullRomSpline(hull);
        MapChange.Invoke();
        SetStartGrid(0);
    }

    public void MoveStartGrid()
    {
        currentStartgridIndex++;
        SetStartGrid(currentStartgridIndex);
    }
    private void SetStartGrid(int index = 0)
    {
        if (startgrid == null)
            return;

        currentStartgridIndex = index;
        startgrid.transform.position = hull[currentStartgridIndex];
        int indexSamePosSmooth = smoothHull.FindIndex(item => item == hull[currentStartgridIndex]);
        if (indexSamePosSmooth == -1)
            Debug.LogWarning("Didn't find matching item in smoothHull list");
        Vector2 forward = (smoothHull[indexSamePosSmooth + 1 % smoothHull.Count] - smoothHull[indexSamePosSmooth])
            .normalized;
        startgrid.transform.up = forward;
        racecar.transform.position = (Vector3)hull[currentStartgridIndex]+Vector3.back*0.5f;
        racecar.transform.up = forward;
        racecar.transform.Translate(-racecar.transform.up*carDistanceToGrid,Space.World);
        var currentScale = startgrid.transform.localScale;
        currentScale.x = GetComponent<TrackMesh>().TrackWidth;
        startgrid.transform.localScale = currentScale;
    }
    
    private List<Vector2> CatmullRomSpline(List<Vector2> dataset)
    {
        List<Vector2> smoothed = new List<Vector2>();

        for (int i = 0; i < dataset.Count; i++)
        {
            Vector2 p0 = dataset[ClampListPos(dataset,i-1)];
            Vector2 p1 = dataset[i];
            Vector2 p2 = dataset[ClampListPos(dataset,i+1)];
            Vector2 p3 = dataset[ClampListPos(dataset,i+2)];

            int steps = smoothingSteps;
            if (Vector2.Distance(p1, p2) < maxDisplacement)
            {
                steps -= subtractIfClose;
            }
            float res = 1f / steps;
            
            for (int j = 0; j < steps; j++)
            {
                float t = j * res;

                Vector2 newPos = CatmullRomInterpolate(p0, p1, p2, p3, t);
                
                smoothed.Add(newPos);
            }
        }

        return smoothed;
    }

    private Vector2 CatmullRomInterpolate(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float t)
    {
        float t2 = t * t;
        float t3 = t * t * t;

        Vector2 a = 2f * v1;
        Vector2 b = v2 - v0;
        Vector2 c = 2f * v0 - 5f * v1 + (4f) * v2 - v3;
        Vector2 d = -v0 + 3f * v1 - 3f * v2 + v3;

        Vector2 pos = 0.5f * (a + b * t + c * t2 + d * t3);
        return pos;
    }
    
    private void CheckAngles(ref List<Vector2> dataset)
    {
        for (int i = 0; i < dataset.Count; i++)
        {
            int prev = (i - 1 < 0) ? dataset.Count - 1 : i - 1;
            int next = (i + 1) % dataset.Count;
            
            float px = dataset[i].x - dataset[prev].x;
            float py = dataset[i].y - dataset[prev].y;
            float pdist = Vector2.Distance(dataset[i], dataset[prev]);
            px /= pdist;
            py /= pdist;
            
            float nx = dataset[i].x - dataset[next].x;
            float ny = dataset[i].y - dataset[next].y;
            nx = -nx;
            ny = -ny;
            float ndist = Vector2.Distance(dataset[i], dataset[next]);
            nx /= ndist;
            ny /= ndist;

            float a = Mathf.Atan2(px * ny - py * nx, px * nx + py * ny);
            if(Mathf.Abs(a*Mathf.Rad2Deg) <= largestTurnDegrees) continue;
            float newA = largestTurnDegrees * Mathf.Sign(a) * Mathf.Deg2Rad;
            float angleDiff = newA - a;
            float cos = Mathf.Cos(angleDiff);
            float sin = Mathf.Sin(angleDiff);
            float newX = nx * cos - ny * sin;
            float newY = nx * sin + ny * cos;
            newX *= ndist;
            newY *= ndist;
            var newPoint = new Vector2(dataset[i].x + newX, dataset[i].y + newY);
            dataset[next] = newPoint;
        }
    }
    
    private void CurvesAndDisplacement(ref List<Vector2> dataset)
    {
        List<Vector2> newDataset = new List<Vector2>();
        Random.InitState((int)DateTime.Now.Ticks);
        for (int i = 0; i < dataset.Count; i++)
        {
            float dispLength = Mathf.Pow(Random.Range(0f, 1f), difficulty) * maxDisplacement;
            var displacement = Vector2.up;
            displacement = Rotate(displacement, Random.Range(0,360f));
            displacement *= dispLength;
            Vector2 midPoint = (dataset[i] + dataset[(i + 1) % dataset.Count]) / 2;
            midPoint += displacement;
            if (Vector2.Distance(midPoint, dataset[i]) < maxDisplacement)
            {
                Debug.Log("Not big");
                int loops = 0;
                var angle = Vector2.Angle(dataset[(i + 1) % dataset.Count] - dataset[i], midPoint);
                while (angle < 60)
                {
                    Debug.Log($"Loop, tiny angle: {angle}");
                    dispLength = Mathf.Pow(Random.Range(0f, 1f), difficulty) * maxDisplacement;
                    displacement = Vector2.up;
                    displacement = Rotate(displacement, Random.Range(0,360f));
                    displacement *= dispLength;
                    midPoint = (dataset[i] + dataset[(i + 1) % dataset.Count]) / 2;
                    midPoint += displacement;
                    angle = Vector2.Angle(dataset[(i + 1) % dataset.Count] - dataset[i], midPoint);
                    if (loops++ >= 10)
                    {
                        Debug.Log("Still tiny angle");
                        dispLength = Mathf.Pow(Random.Range(0f, 1f), difficulty) * maxDisplacement;
                        displacement = (dataset[i] + dataset[(i + 1) % dataset.Count]).normalized;
                        displacement = (midPoint - dataset[i] + midPoint - dataset[(i + 1) % dataset.Count]).normalized;
                        displacement *= dispLength;
                        midPoint = (dataset[i] + dataset[(i + 1) % dataset.Count]) / 2;
                        midPoint += displacement;
                        break;
                    }
                }
            }
            newDataset.Add(dataset[i]);
            newDataset.Add(midPoint);
        }

        dataset = newDataset;
    }
    
    private void PushApart(ref List<Vector2> dataSet)
    {
        float dist = pushSmallestDistance;
        float dist2 = dist * dist;

        for (int i = 0; i < dataSet.Count; i++)
        {
            for (int j = i+1; j < dataSet.Count; j++)
            {
                if ((dataSet[i]-dataSet[j]).sqrMagnitude < dist)
                {
                    float deltaX = dataSet[j].x - dataSet[i].x;
                    float deltaY = dataSet[j].y - dataSet[i].y;
                    float deltaMag = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    deltaX /= deltaMag;
                    deltaY /= deltaMag;

                    float diff = dist - deltaMag;
                    deltaX *= diff;
                    deltaY *= diff;
                    var deltaVector = new Vector2(deltaX, deltaY);
                    dataSet[j] += deltaVector;
                    dataSet[i] -= deltaVector;
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

        int p = l;
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

        Vector2 topRight = new Vector2(groundWidth - groundWidthOffset, groundHeight - groundHeightOffset);
        Vector2 bottomRight = new Vector2(groundWidth - groundWidthOffset, -groundHeight + groundHeightOffset);
        Vector2 topLeft = new Vector2(-groundWidth + groundWidthOffset, groundHeight - groundHeightOffset);
        Vector2 bottomLeft = new Vector2(-groundWidth + groundWidthOffset, -groundHeight + groundHeightOffset);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomLeft, topLeft);

        if (drawPoints)
        {
            Gizmos.color = Color.red;
            foreach (var point in points)
            {
                Gizmos.DrawSphere(point, 1f);
            }
        }

        if (drawHull)
        {
            Gizmos.color = Color.cyan;
            foreach (var point in hull)
            {
                Gizmos.DrawSphere(point, 1f);
            }
        }

        if (drawSmoothHull)
        {
            Gizmos.color = Color.blue;
            foreach (var point in smoothHull)
            {
                Gizmos.DrawSphere(point, 1f);
            }
        }
        
        if (drawPath)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < smoothHull.Count; i++)
            {
                int next = i + 1;
                if (next >= smoothHull.Count)
                    next = 0;

                Gizmos.DrawLine(smoothHull[i], smoothHull[next]);
            }
        }
    }
    
    public static int ClampListPos(List<Vector2> list, int pos)
    {
        if (pos < 0)
        {
            pos = list.Count - 1;
        }

        if (pos > list.Count)
        {
            pos = 1;
        }
        else if (pos > list.Count - 1)
        {
            pos = 0;
        }

        return pos;
    }
    public static Vector2 Rotate(Vector2 v, float delta)
    {
        delta *= Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta));
    }
}

