using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnablePrefab : MonoBehaviour
{
    public float height = 1f;
    public float gizmoRadius = 1f;

    private void OnDrawGizmosSelected()
    {
        Vector3 a = transform.position;
        Vector3 b = transform.position + transform.up * height;

        //Handles.DrawAAPolyLine(a, b);

        //float handleSize = HandleUtility.GetHandleSize(a);
        Gizmos.DrawSphere(a, gizmoRadius);
        //handleSize = HandleUtility.GetHandleSize(b);
        Gizmos.DrawSphere(b, gizmoRadius);

    }
}
