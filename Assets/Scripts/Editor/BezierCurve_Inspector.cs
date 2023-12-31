using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurve_Inspector : Editor
{
    private const int curveResolution = 100;
    private const float directionScale = 0.5f;

    private BezierCurve curve;
    Transform handleTransform;
    Quaternion handleRotation;

    private void OnSceneGUI()
    {
        curve = target as BezierCurve;
        handleTransform = curve.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        Vector3 prevPoint = DrawPoint(0);

        for (int i = 1; i < curve.pointList.Count; i++)
        {
            Vector3 point = DrawPoint(i);

            Handles.color = Color.grey;
            Handles.DrawLine(point, prevPoint);
            prevPoint = point;
        }

        //Vector3 p0 = DrawPoint(0);
        //Vector3 p1 = DrawPoint(1);
        //Vector3 p2 = DrawPoint(2);
        //Vector3 p3 = DrawPoint(3);

        //Handles.color = Color.grey;
        //Handles.DrawLine(p0, p1);
        //Handles.DrawLine(p1, p2);
        //Handles.DrawLine(p2, p3);

        //ShowVelocities();
        //Handles.color = Color.white;
        //Handles.DrawBezier(p0, p3, p1, p2, Color.green, null, 2f);
        DrawCurve();
    }

    private Vector3 DrawPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(curve.pointList[index]);

        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curve, "Point p0 moved");
            EditorUtility.SetDirty(curve);
            curve.pointList[index] = handleTransform.InverseTransformPoint(point);
        }

        return point;
    }

    private void ShowVelocities()
    {
        Handles.color = Color.magenta;

        for (int i = 0; i <= curveResolution; i++)
        {
            Vector3 point = curve.GetNPoint(i / (float)curveResolution);
            Handles.DrawLine(point, point + curve.GetDirection(i / (float)curveResolution) * directionScale);
        }

    }

    private void DrawCurve()
    {
        Handles.color = Color.green;
        Vector3 prevPoint = curve.GetNPoint(0);

        for (int i = 1; i <= curveResolution; i++)
        {
            Vector3 point = curve.GetNPoint(i / (float)curveResolution);
            Handles.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}