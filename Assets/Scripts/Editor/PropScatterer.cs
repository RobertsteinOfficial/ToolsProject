using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;



public class PropScatterer : EditorWindow
{
    [MenuItem("Tools/PropScatterer")]
    public static void OpenWindow() => GetWindow(typeof(PropScatterer));

    public float radius = 2f;
    public int spawnCount = 8;
    public List<GameObject> spawnPrefabs = null;
    public Material previewMaterial;

    SerializedObject so;
    SerializedProperty radiusP;
    SerializedProperty spawnCountP;
    //SerializedProperty spawnPrefabP;
    SerializedProperty previewMatP;

    RandomData[] randomPoints;
    //List<RaycastHit> hitPts = new List<RaycastHit>();
    List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    public GameObject[] prefabs;
    [SerializeField] bool[] selectedPrefabs;

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;

        so = new SerializedObject(this);
        radiusP = so.FindProperty("radius");
        spawnCountP = so.FindProperty("spawnCount");
        //spawnPrefabP = so.FindProperty("spawnPrefab");
        previewMatP = so.FindProperty("previewMaterial");

        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Prefabs/PropScatterer" });
        IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
        prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();

        if (selectedPrefabs == null || selectedPrefabs.Length != prefabs.Length)
        {
            selectedPrefabs = new bool[prefabs.Length];
        }

        GenerateRandomPoints();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void OnGUI()
    {
        so.Update();
        EditorGUILayout.PropertyField(radiusP);
        radiusP.floatValue = radiusP.floatValue.AtLeast(1);

        EditorGUILayout.PropertyField(spawnCountP);
        spawnCountP.intValue = spawnCountP.intValue.AtLeast(1);

        //EditorGUILayout.PropertyField(spawnPrefabP);

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
        if (spawnPrefabs == null) return;
        if (prefabs == null && prefabs.Length == 0) return;

        DrawGUI();

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


        if (TryRaycastFromCamera(cam.up, out Matrix4x4 tangentToWorld))
        {


            spawnPoints = GetSpawnPoints(tangentToWorld);

            if (Event.current.type == EventType.Repaint)
            {
                DrawBrush(tangentToWorld);
                DrawPrefabPreviews(spawnPoints);
            }
        }


        if (Event.current.keyCode == KeyCode.Space && Event.current.type == EventType.KeyDown)
        {
            TrySpawnObjects();
        }
    }

    void GenerateRandomPoints()
    {
        randomPoints = new RandomData[spawnCount];

        for (int i = 0; i < spawnCount; i++)
        {
            randomPoints[i].SetRandomValues(spawnPrefabs);
        }
    }

    List<SpawnPoint> GetSpawnPoints(Matrix4x4 tangentToWorld)
    {
        List<SpawnPoint> poses = new List<SpawnPoint>();
        foreach (RandomData rndDataPoint in randomPoints)
        {
            Ray ptRay = GetTangentRay(rndDataPoint.pointInDisc, tangentToWorld);

            if (Physics.Raycast(ptRay, out RaycastHit ptHit))
            {
                Quaternion randomRot = Quaternion.Euler(0f, 0f, rndDataPoint.randAngle);
                Quaternion rot =
                    Quaternion.LookRotation(ptHit.normal) * (randomRot * Quaternion.Euler(90f, 0f, 0f));

                SpawnPoint pose = new SpawnPoint(ptHit.point, rot, rndDataPoint);

                //Mesh mesh = spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                //previewMaterial.SetPass(0);

                //Graphics.DrawMeshNow
                //    (mesh, Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one));


                poses.Add(pose);
                //Handles.DrawAAPolyLine(ptHit.point, ptHit.point + ptHit.normal);
            }
        }

        return poses;
    }

    bool TryRaycastFromCamera(Vector2 cameraUp, out Matrix4x4 tangentToWorldMatrix)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hitNormal = hit.normal;

            Vector3 hitTangent = Vector3.Cross(hitNormal, cameraUp).normalized;
            Vector3 hitBitangent = Vector3.Cross(hitNormal, hitTangent);

            tangentToWorldMatrix =
                Matrix4x4.TRS(hit.point, Quaternion.LookRotation(hitBitangent, hitNormal), Vector3.one);
            return true;
        }

        tangentToWorldMatrix = default;
        return false;
    }

    Ray GetTangentRay(Vector2 tangentSpacePos, Matrix4x4 tangentToWorld)
    {
        Vector3 right = tangentToWorld.GetRow(0);
        Vector3 forward = tangentToWorld.GetRow(2);
        Vector3 up = tangentToWorld.GetRow(1);

        Vector3 rayOrigin = tangentToWorld.GetPosition() + (right * tangentSpacePos.x + forward * tangentSpacePos.y) * radius;
        Vector3 n = up;
        rayOrigin += n * 2;
        Vector3 rayDirection = -n;

        return new Ray(rayOrigin, rayDirection);
    }

    void TrySpawnObjects()
    {
        if (spawnPrefabs == null || spawnPrefabs.Count == 0) return;

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.isValid == false) continue;

            GameObject thingToSpawn = (GameObject)PrefabUtility.InstantiatePrefab(spawnPoint.spawnData.spawnPrefab);
            Undo.RegisterCreatedObjectUndo(thingToSpawn, "Object Spawn");
            thingToSpawn.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        GenerateRandomPoints();
    }

    void DrawGUI()
    {
        Handles.BeginGUI();

        Rect rect = new Rect(8, 8, 50, 50);

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];
            Texture icon = AssetPreview.GetAssetPreview(prefab);

            EditorGUI.BeginChangeCheck();
            selectedPrefabs[i] = GUI.Toggle(rect, selectedPrefabs[i], new GUIContent(icon));

            if (EditorGUI.EndChangeCheck())
            {
                spawnPrefabs.Clear();

                for (int j = 0; j < prefabs.Length; j++)
                {
                    if (selectedPrefabs[j])
                        spawnPrefabs.Add(prefabs[j]);
                }

                GenerateRandomPoints();
            }

            rect.y += rect.height + 2;
        }

        Handles.EndGUI();
    }

    void DrawPoint(Vector3 pos)
    {
        Handles.SphereHandleCap(-1, pos, Quaternion.identity, 0.1f, EventType.Repaint);
    }

    void DrawPrefabPreviews(List<SpawnPoint> spawnPoints)
    {
        if (spawnPrefabs == null || spawnPrefabs.Count == 0) return;

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnData.spawnPrefab == null || !spawnPoint.isValid) continue;

            Matrix4x4 poseToWorldMtx = Matrix4x4.TRS(spawnPoint.position, spawnPoint.rotation, Vector3.one);
            MeshFilter[] filters = spawnPoint.spawnData.spawnPrefab.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter filter in filters)
            {
                Matrix4x4 childToPose = filter.transform.localToWorldMatrix;
                Matrix4x4 childToWorldMtx = poseToWorldMtx * childToPose;

                Mesh mesh = filter.sharedMesh;
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                mat.SetPass(0);
                Graphics.DrawMeshNow(mesh, childToWorldMtx);
            }

            DrawPoint(spawnPoint.position);
        }
    }

    void DrawBrush(Matrix4x4 tangentToWorld)
    {
        const int circleDetail = 128;
        Vector3[] points = new Vector3[circleDetail];
        const float DUEPI = 6.28318530718f;

        for (int i = 0; i < circleDetail; i++)
        {
            float t = i / (float)circleDetail;

            float angRad = t * DUEPI;
            Vector2 dir = new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad));

            Ray r = GetTangentRay(dir, tangentToWorld);


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

        Vector3 hitPos = tangentToWorld.GetPosition();

        Handles.color = Color.red;
        Handles.DrawAAPolyLine(6, hitPos, hitPos + (Vector3)tangentToWorld.GetRow(0));
        Handles.color = Color.green;
        Handles.DrawAAPolyLine(6, hitPos, hitPos + (Vector3)tangentToWorld.GetRow(2));
        Handles.color = Color.blue;
        Handles.DrawAAPolyLine(6, hitPos, hitPos + (Vector3)tangentToWorld.GetRow(1));

        Handles.color = Color.white;

    }

}

public struct RandomData
{
    public Vector2 pointInDisc;
    public float randAngle;
    public GameObject spawnPrefab;

    public void SetRandomValues(List<GameObject> prefabs)
    {
        pointInDisc = Random.insideUnitCircle;
        randAngle = Random.value * 360;
        spawnPrefab = prefabs.Count == 0 ? null : prefabs[Random.Range(0, prefabs.Count)];
    }
}

public struct SpawnPoint
{
    public RandomData spawnData;
    public Vector3 position;
    public Quaternion rotation;
    public bool isValid;

    public SpawnPoint(Vector3 position, Quaternion rotation, RandomData spawnData)
    {
        this.position = position;
        this.rotation = rotation;
        this.spawnData = spawnData;

        if (spawnData.spawnPrefab == null)
        {
            isValid = false;
            return;
        }

        SpawnablePrefab spawnablePrefab = spawnData.spawnPrefab.GetComponent<SpawnablePrefab>();

        if (spawnablePrefab == null)
            isValid = true;
        else
        {
            float h = spawnablePrefab.height;
            Ray ray = new Ray(position, rotation * Vector3.up);
            isValid = (Physics.Raycast(ray, h) == false && spawnData.spawnPrefab != null);
        }

    }
}
