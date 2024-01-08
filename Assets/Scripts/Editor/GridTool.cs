using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridTool : EditorWindow
{
    public enum GridType
    {
        Cartesian,
        Polar
    }

    [MenuItem("Tools/GridSnapper")]
    public static void OpenWindow() => GetWindow<GridTool>("GridSnapper");

    public GridType gridType = GridType.Cartesian;
    public int gridSize = 10;
    public int angularDivisions = 16;

    const float gridExtent = 16;
    const float DUEPI = 6.28318530718f;

    SerializedObject so;
    SerializedProperty gridSizeP;
    SerializedProperty gridTypeP;
    SerializedProperty angularDivisionsP;


    private void OnEnable()
    {
        so = new SerializedObject(this);
        gridSizeP = so.FindProperty("gridSize");
        gridTypeP = so.FindProperty("gridType");
        angularDivisionsP = so.FindProperty("angularDivisions");

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;

        gridType = (GridType)EditorPrefs.GetInt("snapper_tool_gridType", 0);
        gridSize = EditorPrefs.GetInt("snapper_tool_gridSize", 10);
        angularDivisions = EditorPrefs.GetInt("snapper_tool_angularDivisions", 16);
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;

        EditorPrefs.SetInt("snapper_tool_gridType", (int)gridType);
        EditorPrefs.SetInt("snapper_tool_gridSize", gridSize);
        EditorPrefs.SetInt("snapper_tool_angularDivisions", angularDivisions);

    }

    private void OnGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(gridTypeP);
        EditorGUILayout.PropertyField(gridSizeP);

        if (gridType == GridType.Polar)
        {
            EditorGUILayout.PropertyField(angularDivisionsP);
            angularDivisionsP.intValue = Mathf.Max(4, angularDivisionsP.intValue);
        }

        so.ApplyModifiedProperties();

        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Snap Selection"))
            {
                SnapStuff();
            }
        }
    }


    private void SnapStuff()
    {

        foreach (GameObject item in Selection.gameObjects)
        {
            Undo.RecordObject(item.transform, "snap stuff");
            Vector3 pos = GetSnappedPosition(item.transform.position);
            item.transform.position = pos;
        }
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            if (gridType == GridType.Cartesian)
                DrawCartesian();
            else
                DrawPolar();
        }
    }

    void DrawCartesian()
    {
        int lineCount = Mathf.RoundToInt((gridExtent * 2) / gridSize);

        if (lineCount % 2 == 0)
        {
            lineCount++;
        }

        int halfLineCount = lineCount / 2;

        for (int i = 0; i < lineCount; i++)
        {
            int offsetIndex = i - halfLineCount;

            float xCoord = offsetIndex * gridSize;
            float zCoord0 = halfLineCount * gridSize;
            float zCoord1 = -halfLineCount * gridSize;

            Vector3 p0 = new Vector3(xCoord, 0f, zCoord0);
            Vector3 p1 = new Vector3(xCoord, 0f, zCoord1);

            Handles.DrawAAPolyLine(p0, p1);

            Vector3 p2 = new Vector3(zCoord0, 0f, xCoord);
            Vector3 p3 = new Vector3(zCoord1, 0f, xCoord);
            Handles.DrawAAPolyLine(p2, p3);
        }
    }

    void DrawPolar()
    {
        int ringCount = Mathf.RoundToInt((gridExtent / gridSize));

        for (int i = 0; i < ringCount; i++)
        {
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, i * gridSize);
        }


        for (int i = 0; i < angularDivisions; i++)
        {
            float t = i / (float)angularDivisions;
            float angRad = t * DUEPI;

            float x = Mathf.Cos(angRad);
            float z = Mathf.Sin(angRad);

            Vector3 dir = new Vector3(x, 0f, z);

            float outerRadius = (ringCount - 1) * gridSize;

            Handles.DrawAAPolyLine(Vector3.zero, dir * outerRadius);
        }
    }

    Vector3 GetSnappedPosition(Vector3 startPos)
    {
        if (gridType == GridType.Cartesian)
            return startPos.Round(gridSize);
        else
        {
            Vector2 v = new Vector2(startPos.x, startPos.z);
            float dist = v.magnitude;
            float distSnapped = dist.Round(gridSize);

            float angRad = Mathf.Atan2(v.y, v.x);
            float angTurns = angRad / DUEPI;
            float angSnapped = angTurns.Round(1f / angularDivisions);

            float angRadSnapped = angSnapped * DUEPI;

            Vector2 dirSnapped = new Vector2(Mathf.Cos(angRadSnapped), Mathf.Sin(angRadSnapped)) * distSnapped;


            return new Vector3(dirSnapped.x, startPos.y, dirSnapped.y);
        }
    }
}
