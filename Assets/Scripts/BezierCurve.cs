using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public Vector3[] points;

    public List<Vector3> pointList;

    public void Reset()
    {
        points = new Vector3[] { Vector3.right, Vector3.right * 2, Vector3.right * 3, Vector3.right * 4 };
    }

    public Vector3 GetPoint(float t)
    {
        return transform.TransformPoint(BezierMath.GetPoint(points[0], points[1], points[2], points[3], t));
    }

    public Vector3 GetNPoint(float t)
    {
        return transform.TransformPoint(BezierMath.GetPoint(pointList, t));
    }

    public Vector3 GetVelocity(float t)
    {
        return transform.TransformPoint(BezierMath.GetFirstDerivative(points[0], points[1], points[2], points[3], t));
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }
}
