using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spline))]
public class Spline_Inspector : Editor
{
    private static Color[] modeColors =
    {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private const int curveResolution = 10;
    private const float directionScale = 0.5f;
    private const int stepsPerCurve = 10;
    private const float handleSize = 0.05f;
    private const float pickSize = 0.08f;

    private int selectedIndex = -1;
    private Spline spline;
    Transform handleTransform;
    Quaternion handleRotation;

    public override void OnInspectorGUI()
    {
        spline = target as Spline;

        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
        {
            DrawPointInInspector();
        }

        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            //EditorUtility.SetDirty(spline);
        }
    }

    private void OnSceneGUI()
    {
        spline = target as Spline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = DrawPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {

            Vector3 p1 = DrawPoint(i);
            Vector3 p2 = DrawPoint(i + 1);
            Vector3 p3 = DrawPoint(i + 2);

            Handles.color = Color.grey;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.color = Color.white;
            Handles.DrawBezier(p0, p3, p1, p2, Color.green, null, 2f);
            p0 = p3;
        }
        ShowVelocities();
    }

    private Vector3 DrawPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));

        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (Handles.Button(point, handleRotation, handleSize, pickSize, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index)
        {

            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Point p0 moved");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }

        return point;
    }

    private void ShowVelocities()
    {
        Handles.color = Color.magenta;


        int steps = stepsPerCurve * spline.CurveCount;
        for (int i = 0; i <= steps; i++)
        {
            Vector3 point = spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
        }

    }

    private void DrawPointInInspector()
    {
        GUILayout.Label("Point");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            spline.SetControlPoint(selectedIndex, point);
        }

        EditorGUI.BeginChangeCheck();
        CurveControlPointMode mode =
            (CurveControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
        }
    }
}