using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridTool : EditorWindow
{
    [MenuItem("Tools/GridSnapper")]
    public static void OpenWindow() => GetWindow<GridTool>("GridSnapper");

    public int gridSize = 10;
    public Transform tr;

    SerializedObject so;
    SerializedProperty gridSizeP;
    SerializedProperty trp;


    private void OnEnable()
    {
        so = new SerializedObject(this);
        gridSizeP = so.FindProperty("gridSize");
        trp = so.FindProperty("tr");

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;

    }

    private void OnGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(gridSizeP);
        so.ApplyModifiedProperties();

        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Snap Selection"))
            {
                SnapStuff();
            }
        }

        EditorGUILayout.ObjectField(trp);
    }



    private void SnapStuff()
    {

        foreach (GameObject item in Selection.gameObjects)
        {
            Undo.RecordObject(item.transform, "snap stuff");
            Vector3 pos = item.transform.position.Round(gridSize);
            item.transform.position = pos;
        }
    }

    private void DuringSceneGUI(SceneView sceneView)
    {

    }
}
