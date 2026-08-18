using UnityEngine;
using UnityEditor;

public class SceneEditor : EditorWindow
{
    public GameObject[] prefabList = new GameObject[0];
    private int selectedIndex = 0;
    private float gridSize = 1f;
    private bool isPainting = false;

    [MenuItem("Window/Scene Editor")]
    public static void ShowWindow()
    {
        GetWindow<SceneEditor>("Scene Editor");
    }

    // BOA PRÁTICA: Inscreve o evento apenas uma vez ao abrir a janela
    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Editor Settings", EditorStyles.boldLabel);
        gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);

        GUILayout.Space(10);
        GUILayout.Label("Prefab List", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("prefabList");
        EditorGUILayout.PropertyField(stringsProperty, new GUIContent("Blocos"), true);
        so.ApplyModifiedProperties();

        if (prefabList == null || prefabList.Length == 0)
        {
            EditorGUILayout.HelpBox("Adicione pelo menos um Prefab na lista acima para começar.", MessageType.Warning);
            return;
        }

        GUILayout.Space(15);
        GUILayout.Label("Selecione o Bloco para Posicionar:", EditorStyles.boldLabel);

        string[] options = new string[prefabList.Length];
        for (int i = 0; i < prefabList.Length; i++)
        {
            options[i] = prefabList[i] != null ? prefabList[i].name : "Vazio";
        }

        selectedIndex = GUILayout.SelectionGrid(selectedIndex, options, 3);

        GUILayout.Space(15);
        
        GUI.backgroundColor = isPainting ? Color.green : Color.red;
        isPainting = GUILayout.Toggle(isPainting, "Modo Pintura: " + (isPainting ? "ATIVADO" : "DESATIVADO"), "Button", GUILayout.Height(35));
        GUI.backgroundColor = Color.white;

        // Força a atualização da Scene View quando o modo de pintura muda
        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPainting) return;

        Event e = Event.current;

        // Bloqueia a seleção padrão da Unity na Scene View para não atrapalhar a pintura
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        // Detecta clique ou arrasto com o botão esquerdo
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 spawnPos = Vector3.zero;
            bool hitPosFound = false;

            // 1. Tenta colidir com objetos existentes na cena
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                spawnPos = SnapToGrid(hit.point + hit.normal * (gridSize * 0.5f));
                hitPosFound = true;
            }
            // 2. FALLBACK: Se a cena estiver vazia, projeta no plano horizontal Y = 0
            else
            {
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    spawnPos = SnapToGrid(ray.GetPoint(enter));
                    hitPosFound = true;
                }
            }

            if (hitPosFound && !BlockExistsAt(spawnPos))
            {
                SpawnBlock(spawnPos);
                e.Use(); // Consome o evento para evitar cliques indesejados
            }
        }
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }

    private bool BlockExistsAt(Vector3 pos)
    {
        // Verifica colisores na posição para evitar sobreposição
        Collider[] colliders = Physics.OverlapSphere(pos, gridSize * 0.4f);
        return colliders.Length > 0;
    }

    private void SpawnBlock(Vector3 position)
    {
        if (prefabList == null || prefabList.Length == 0) return;

        GameObject prefabToSpawn = prefabList[selectedIndex];
        if (prefabToSpawn == null) return;

        GameObject spawnedObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
        spawnedObj.transform.position = position;

        Undo.RegisterCreatedObjectUndo(spawnedObj, "Pintar Bloco");
    }
}
