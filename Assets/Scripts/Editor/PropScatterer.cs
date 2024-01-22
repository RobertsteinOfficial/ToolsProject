using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;



public class PropScatterer : EditorWindow
{
    [MenuItem("Tools/PropScatterer")]
    public static void OpenWindow() => GetWindow(typeof(PropScatterer));

    public float radius = 2f;
    public int spawnCount = 8;
    public GameObject spawnPrefab = null;
    public Material previewMaterial;

    SerializedObject so;
    SerializedProperty radiusP;
    SerializedProperty spawnCountP;
    SerializedProperty spawnPrefabP;
    SerializedProperty previewMatP;

    RandomData[] randomPoints;
    //List<RaycastHit> hitPts = new List<RaycastHit>();
    List<Pose> posePts = new List<Pose>();
    GameObject[] prefabs;


    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;

        so = new SerializedObject(this);
        radiusP = so.FindProperty("radius");
        spawnCountP = so.FindProperty("spawnCount");
        spawnPrefabP = so.FindProperty("spawnPrefab");
        previewMatP = so.FindProperty("previewMaterial");

        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Prefabs/PropScatterer" });
        IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
        prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();


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

        EditorGUILayout.PropertyField(spawnPrefabP);

        EditorGUILayout.PropertyField(previewMatP);

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
        if (prefabs == null && prefabs.Length == 0) return;


        Handles.BeginGUI();

        Rect rect = new Rect(8, 8, 50, 50);

        foreach (GameObject prefab in prefabs)
        {
            Texture icon = AssetPreview.GetAssetPreview(prefab);

            //if (GUI.Button(rect, new GUIContent(prefab.name, icon)))
            //{
            //    spawnPrefab = prefab;
            //}

            if (GUI.Toggle(rect, spawnPrefab == prefab, new GUIContent(icon)))
            {
                spawnPrefab = prefab;
            }

            rect.y += rect.height + 2;
        }

        Handles.EndGUI();

        if (spawnPrefab == null) return;

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

        if (Event.current.keyCode == KeyCode.Space && Event.current.type == EventType.KeyDown)
        {
            TrySpawnObjects();
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
                rayOrigin += hitNormal * 5;
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

            Handles.color = Color.white;

            posePts.Clear();
            foreach (RandomData rndDataPoint in randomPoints)
            {
                Ray ptRay = GetTangentRay(rndDataPoint.pointInDisc);

                if (Physics.Raycast(ptRay, out RaycastHit ptHit))
                {
                    Quaternion randomRot = Quaternion.Euler(0f, 0f, rndDataPoint.randAngle);
                    Quaternion rot =
                        Quaternion.LookRotation(ptHit.normal) * (randomRot * Quaternion.Euler(90f, 0f, 0f));

                    Pose pose = new Pose(ptHit.point, rot);

                    //Mesh mesh = spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                    //previewMaterial.SetPass(0);

                    //Graphics.DrawMeshNow
                    //    (mesh, Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one));

                    Matrix4x4 poseToWorldMtx = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
                    MeshFilter[] filters = spawnPrefab.GetComponentsInChildren<MeshFilter>();

                    foreach (MeshFilter filter in filters)
                    {
                        Matrix4x4 childToPose = filter.transform.localToWorldMatrix;
                        Matrix4x4 childToWorldMtx = poseToWorldMtx * childToPose;

                        Mesh mesh = filter.sharedMesh;
                        Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                        mat.SetPass(0);
                        Graphics.DrawMeshNow(mesh, childToWorldMtx);
                    }


                    DrawPoint(ptHit.point);
                    posePts.Add(pose);
                    //Handles.DrawAAPolyLine(ptHit.point, ptHit.point + ptHit.normal);
                }
            }
        }

    }

    void GenerateRandomPoints()
    {
        randomPoints = new RandomData[spawnCount];

        for (int i = 0; i < spawnCount; i++)
        {
            randomPoints[i].SetRandomValues();
        }
    }

    void DrawPoint(Vector3 pos)
    {
        Handles.SphereHandleCap(-1, pos, Quaternion.identity, 0.1f, EventType.Repaint);
    }

    void TrySpawnObjects()
    {
        if (spawnPrefab == null) return;

        foreach (Pose pose in posePts)
        {


            GameObject thingToSpawn = (GameObject)PrefabUtility.InstantiatePrefab(spawnPrefab);
            Undo.RegisterCreatedObjectUndo(thingToSpawn, "Object Spawn");
            thingToSpawn.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        GenerateRandomPoints();
    }
}


public struct RandomData
{
    public Vector2 pointInDisc;
    public float randAngle;

    public void SetRandomValues()
    {
        pointInDisc = Random.insideUnitCircle;
        randAngle = Random.value * 360;
    }
}
