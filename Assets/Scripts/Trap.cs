using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class Trap : MonoBehaviour
{
    public static Color overrideColor;

    public int damage = 10;

    [Range(1, 20)]
    public float radius = 5f;

    public static List<Spawner> spawners = new List<Spawner>();

    public TestChildClass testClass;

    private void Start()
    {

    }

    public static void OverrideColors()
    {
        for (int i = 0; i < spawners.Count; i++)
        {
            spawners[i].ApplyColour(overrideColor);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;


        foreach (var spawner in spawners)
        {
            //Gizmos.DrawLine(transform.position, spawner.transform.position);
            //Handles.DrawAAPolyLine(transform.position, spawner.transform.position);

            Vector3 trapPos = transform.position;
            Vector3 spawnerPos = spawner.transform.position;
            float halfPoint = (trapPos.y - spawnerPos.y) * .5f;
            Vector3 offset = Vector3.up * halfPoint;

            Handles.DrawBezier(
                trapPos,
                spawnerPos,
                trapPos - offset,
                spawnerPos + offset,
                Color.blue,
                EditorGUIUtility.whiteTexture,
                1f);
        }
    }
#endif

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);

    }
}

[System.Serializable]
public class TestClass : ScriptableObject
{
    public Vector3 pos;
    public Color col;
}

[System.Serializable]
public class TestChildClass : TestClass
{
    public int i;
}

