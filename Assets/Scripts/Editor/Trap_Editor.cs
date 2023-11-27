using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Trap))]
public class Trap_Editor : Editor
{
    SerializedObject so;
    SerializedProperty propRadius;
    SerializedProperty propDamage;
    SerializedProperty propCol;

    private void OnEnable()
    {
        //        SerializedObject
        //       SerializedProperty


        so = serializedObject;
        propRadius = so.FindProperty("radius");
        propDamage = so.FindProperty("damage");
        propCol = so.FindProperty("overrideColor");
    }


    public override void OnInspectorGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(propRadius);
        EditorGUILayout.PropertyField(propDamage);
        if(so.ApplyModifiedProperties())
        {
            Trap.OverrideColors();
        }
    }

}
