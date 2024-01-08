using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GridSnapper
{
    [MenuItem("Edit/Snapper %&S", isValidateFunction: true)]
    public static bool SnapValidation()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem("Edit/Snapper %&S")]
    public static void Snap()
    {
        foreach (GameObject item in Selection.gameObjects)
        {
            Undo.RecordObject(item.transform, "snap stuff");
            Vector3 pos = item.transform.position.Round();
            item.transform.position = pos;
        }
    }


}
