using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    [SerializeField] private Vector3[] points;
    [SerializeField] private CurveControlPointMode[] modes;

    public int ControlPointCount { get { return points.Length; } }
    public Vector3 GetControlPoint(int index) { return points[index]; }
    public void SetControlPoint(int index, Vector3 point)
    {
        points[index] = point;
        EnforceMode(index);
    }

    public int CurveCount { get { return (points.Length - 1) / 3; } }

    public void Reset()
    {
        points = new Vector3[] { Vector3.right, Vector3.right * 2, Vector3.right * 3, Vector3.right * 4 };
        modes = new CurveControlPointMode[] { CurveControlPointMode.Free, CurveControlPointMode.Free };
    }

    public Vector3 GetPoint(float t)
    {
        int i;

        if (t >= 1)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        return transform.TransformPoint(BezierMath.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;

        if (t >= 1)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }

        return transform.TransformPoint(BezierMath.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public void AddCurve()
    {
        Vector3 point = points[points.Length - 1];

        Array.Resize(ref points, points.Length + 3);
        point.x += 1f;
        points[points.Length - 3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
    }

    public CurveControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, CurveControlPointMode mode)
    {
        modes[(index + 1) / 3] = mode;
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        CurveControlPointMode mode = modes[modeIndex];

        if (mode == CurveControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Length - 1)
        {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;

        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            enforcedIndex = middleIndex + 1;
        }
        else
        {
            fixedIndex = middleIndex + 1;
            enforcedIndex = middleIndex - 1;
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];


        if (mode == CurveControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }

        points[enforcedIndex] = middle + enforcedTangent;
    }
}


public enum CurveControlPointMode
{
    Free,
    Aligned,
    Mirrored
}
