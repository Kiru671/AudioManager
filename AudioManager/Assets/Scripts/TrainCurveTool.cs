using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TrainCurveTool : MonoBehaviour
{
    [SerializeField]public Curve[] curves;
    public bool drawOnUpdate;
    public float totalLength;
    [SerializeField]public CurvePoint[] points;
    private CurvePoint prevPoint;

    private void Update()
    {
#if UNITY_EDITOR
        if (drawOnUpdate && curves[0] != null && curves[0].start != null && curves[0].end != null)
            PlotAllCurves();
#endif
    }


    public Vector3 GetPointAtLength(float length)
    {
        length = length % totalLength;

        float startLength = 0;
        float endLength = points[1].length;
        int startIndex = 0;

        for (int i = 1; (i < points.Length - 1 && endLength <= length); i++)
        {
            startLength += points[i].length;
            endLength += points[i+1].length;
            startIndex = i;
        }
        
        float diff = endLength - startLength;
        float ratio = (length - startLength) / diff;
        Debug.Log($"Ratio detected to be : {ratio}");
        Vector3 point = Vector3.Lerp(points[startIndex].position, points[startIndex+1].position, ratio);
        
        return point;
        
    }

    public void PlotAllCurves()
    {
        List<CurvePoint> allPoints = new List<CurvePoint>();
        totalLength = 0;
        prevPoint = new CurvePoint(0, curves[0].start.position);
        for (int i = 0; i < curves.Length; i++)
        {
            List<Vector3> outerPoints = new List<Vector3>();

            outerPoints.Add(curves[i].start.position);
            for (int j = 0; j < curves[i].handles.Length; j++)
            {
                outerPoints.Add(curves[i].handles[j].position);
            }
            outerPoints.Add(curves[i].end.position);

        
            allPoints.Add(prevPoint);
            float t = 1f / curves[i].pointCount;
            while (t < 1f)
            {
                CurvePoint newPoint = GetPoint(t, outerPoints);
                newPoint.length = (newPoint.position - prevPoint.position).magnitude;
                totalLength += newPoint.length;
                prevPoint = newPoint;
                allPoints.Add(newPoint);
                t += 1f/curves[i].pointCount;
            }
        }
        points = allPoints.ToArray();
    }
    
    public void PlotCurve()
    {

    }
    
    private CurvePoint GetPoint(float t, List<Vector3> outPoints)
    {
        CurvePoint point = new CurvePoint(0, Vector3.zero);
        List<Vector3> outerPoints = new List<Vector3>(outPoints);
        List<Vector3> innerPoints = new List<Vector3>();


        while (outerPoints.Count > 2)
        {
            for (int i = 0; i < outerPoints.Count-1; i++)
            {
                Vector3 innerPoint = Vector3.Lerp(outerPoints[i], outerPoints[i + 1], t);
                innerPoints.Add(innerPoint);
            }

            outerPoints = new List<Vector3>(innerPoints);
            innerPoints.Clear();
        }

        point.position = Vector3.Lerp(outerPoints[0], outerPoints[1], t);

        return point;
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (points != null && points.Length > 0)
        {

            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
            
        }
    }
#endif

    
}
[System.Serializable]
public class CurvePoint
{
    public float length;
    public Vector3 position;

    public CurvePoint(float length, Vector3 position)
    {
        this.length = length;
        this.position = position;
    }
}
[System.Serializable]
public class Curve
{
    public Transform start, end;
    public Transform[] handles;
    public int pointCount;
}