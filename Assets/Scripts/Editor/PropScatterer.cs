using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class PropScatterer : EditorWindow
{
    [MenuItem("Tools/PropScatterer")]
    public static void OpenWindow() => GetWindow(typeof(PropScatterer));

    public float radius = 2f;
    public int spawnCount = 8;
    public GameObject spawnPrefab = null;

    SerializedObject so;
    SerializedProperty radiusP;
    SerializedProperty spawnCountP;
    SerializedProperty spawnPrefabP;

    Vector2[] randomPoints;

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;

        so = new SerializedObject(this);
        radiusP = so.FindProperty("radius");
        spawnCountP = so.FindProperty("spawnCount");
        spawnPrefabP = so.FindProperty("spawnPrefab");

        GenerateRandomPoints();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(radiusP);
        radiusP.floatValue = radiusP.floatValue.AtLeast(1);

        EditorGUILayout.PropertyField(spawnCountP);
        spawnCountP.intValue = spawnCountP.intValue.AtLeast(1);

        EditorGUILayout.PropertyField (spawnPrefabP);

        if (so.ApplyModifiedProperties())
        {
            GenerateRandomPoints();
            SceneView.RepaintAll();
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl(null);
            Repaint();
        }



    }

    void DuringSceneGUI(SceneView sceneView)
    {


        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        Transform cam = sceneView.camera.transform;

        if (Event.current.type == EventType.MouseMove)
        {
            sceneView.Repaint();
        }

        bool isHoldingAlt = (Event.current.modifiers & EventModifiers.Alt) != 0;

        if (Event.current.type == EventType.ScrollWheel && isHoldingAlt == true)
        {
            float scrollDirection = Mathf.Sign(Event.current.delta.y);

            so.Update();
            radiusP.floatValue *= 1f - scrollDirection * 0.05f;


            if (so.ApplyModifiedProperties())
            {
                Repaint();
            }

            Event.current.Use();
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            const int circleDetail = 128;
            Vector3[] points = new Vector3[circleDetail];
            const float DUEPI = 6.28318530718f;

            Vector3 hitNormal = hit.normal;
            Vector3 hitTangent = Vector3.Cross(hitNormal, cam.up).normalized;
            Vector3 hitBitangent = Vector3.Cross(hitNormal, hitTangent);

            Ray GetTangentRay(Vector2 tangentSpacePos)
            {
                Vector3 rayOrigin = hit.point + (hitTangent * tangentSpacePos.x + hitBitangent * tangentSpacePos.y) * radius;
                rayOrigin += hitNormal * 2;
                Vector3 rayDirection = -hit.normal;

                return new Ray(rayOrigin, rayDirection);
            }


            for (int i = 0; i < circleDetail; i++)
            {
                float t = i / (float)circleDetail;

                float angRad = t * DUEPI;
                Vector2 dir = new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad));
                Ray r = GetTangentRay(dir);
                

                if (Physics.Raycast(r, out RaycastHit cHit))
                {
                    points[i] = cHit.point + cHit.normal * 0.02f;
                }
                else
                {
                    points[i] = r.origin;
                }

            }

            Handles.DrawAAPolyLine(points);

            //Handles.DrawWireDisc(hit.point, hit.normal, radius);



            Handles.color = Color.red;
            Handles.DrawAAPolyLine(6, hit.point, hit.point + hitTangent);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(6, hit.point, hit.point + hitBitangent);
            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(6, hit.point, hit.point + hitNormal);



            foreach (Vector2 p in randomPoints)
            {

                Ray ptRay = GetTangentRay(p);

                if (Physics.Raycast(ptRay, out RaycastHit ptHit))
                {
                    DrawPoint(ptHit.point);
                    Handles.DrawAAPolyLine(ptHit.point, ptHit.point + ptHit.normal);
                }

            }
        }

    }

    void GenerateRandomPoints()
    {
        randomPoints = new Vector2[spawnCount];

        for (int i = 0; i < spawnCount; i++)
        {
            randomPoints[i] = Random.insideUnitCircle;
        }
    }

    void DrawPoint(Vector3 pos)
    {
        Handles.SphereHandleCap(-1, pos, Quaternion.identity, 0.1f, EventType.Repaint);
    }


}
