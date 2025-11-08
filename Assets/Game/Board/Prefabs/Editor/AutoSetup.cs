using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AutoSetup
{
    [MenuItem("Tools/Game/Generate Default Prefabs, Data, and Scene")] 
    public static void GenerateAll()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            EnsureFolders();
            // Create prefabs
            var normalPrefab = CreateTilePrefab("Assets/Game/Board/Prefabs/NormalTile_Prefab.prefab", new Color(0.75f, 0.75f, 0.75f), new Vector3(1f, 0.2f, 1f));
            var obstaclePrefab = CreateTilePrefab("Assets/Game/Board/Prefabs/ObstacleTile_Prefab.prefab", new Color(0.3f, 0.3f, 0.3f), new Vector3(1f, 1f, 1f));
            var exitPrefab = CreateExitPrefab("Assets/Game/Board/Prefabs/ExitTile_Prefab.prefab", new Color(1f, 0.9f, 0.2f));

            var playerPrefab = CreateUnitPrefab("Assets/Game/Units/Prefabs/Player_Soldier.prefab", new Color(0.2f, 0.9f, 0.2f));
            var enemyPrefab = CreateUnitPrefab("Assets/Game/Units/Prefabs/Enemy_Slime.prefab", new Color(0.9f, 0.2f, 0.2f));

            // Create UnitData assets
            var playerUnit = ScriptableObject.CreateInstance<UnitData>();
            playerUnit.name = "Player_Soldier";
            playerUnit.id = "Player_Soldier";
            playerUnit.HP = 8;
            playerUnit.Attack = 2;
            playerUnit.AttackSpeed = 1.2f;
            playerUnit.Range = 1.25f;
            playerUnit.MoveSpeed = 2.5f;
            playerUnit.prefab = playerPrefab;
            playerUnit.defaultTeam = Team.Player;
            AssetDatabase.CreateAsset(playerUnit, "Assets/Game/Data/Player_Soldier.asset");

            var enemyUnit = ScriptableObject.CreateInstance<UnitData>();
            enemyUnit.name = "Enemy_Slime";
            enemyUnit.id = "Enemy_Slime";
            enemyUnit.HP = 5;
            enemyUnit.Attack = 1;
            enemyUnit.AttackSpeed = 1.0f;
            enemyUnit.Range = 1.25f;
            enemyUnit.MoveSpeed = 2.0f;
            enemyUnit.prefab = enemyPrefab;
            enemyUnit.defaultTeam = Team.Enemy;
            AssetDatabase.CreateAsset(enemyUnit, "Assets/Game/Data/Enemy_Slime.asset");

            // Create two BoardData assets
            var level0 = CreateBoardData("Assets/Game/Data/Level_0_Data.asset", new Vector2Int(8, 4), enemyUnit,
                new[] { new Vector2Int(2, 1), new Vector2Int(4, 2), new Vector2Int(6, 0) },
                new[] { new Vector2Int(5, 1), new Vector2Int(6, 2), new Vector2Int(7, 3) });

            var level1 = CreateBoardData("Assets/Game/Data/Level_1_Data.asset", new Vector2Int(8, 4), enemyUnit,
                new[] { new Vector2Int(1, 0), new Vector2Int(3, 1), new Vector2Int(5, 2), new Vector2Int(6, 3) },
                new[] { new Vector2Int(4, 0), new Vector2Int(4, 3) });

            // Build scene
            BuildAndSaveScene(normalPrefab, obstaclePrefab, exitPrefab, level0, level1);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static void EnsureFolders()
    {
        CreateFolderRecursive("Assets/Scenes");
        CreateFolderRecursive("Assets/Game/Data");
        CreateFolderRecursive("Assets/Game/Board/Prefabs");
        CreateFolderRecursive("Assets/Game/Units/Prefabs");
        CreateFolderRecursive("Assets/Game/Board/Prefabs/Materials");
    }

    private static void CreateFolderRecursive(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        var cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(cur, parts[i]);
            }
            cur = next;
        }
    }

    private static Material CreateMaterial(string path, Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static GameObject CreateTilePrefab(string path, Color color, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = scale;
        var mat = CreateMaterial(path.Replace(".prefab", "_Mat.mat").Replace("Prefabs", "Prefabs/Materials"), color);
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = mat;
        // ensure collider present (from primitive)
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return (GameObject)prefab;
    }

    private static GameObject CreateExitPrefab(string path, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(1f, 0.15f, 1f);
        var mat = CreateMaterial(path.Replace(".prefab", "_Mat.mat").Replace("Prefabs", "Prefabs/Materials"), color);
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = mat;
        go.AddComponent<ExitTileLink>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return (GameObject)prefab;
    }

    private static GameObject CreateUnitPrefab(string path, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        var mat = CreateMaterial(path.Replace(".prefab", "_Mat.mat").Replace("Prefabs", "Prefabs/Materials"), color);
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = mat;
        go.AddComponent<UnitInstanceComponent>();
        go.AddComponent<BasicCombatant>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return (GameObject)prefab;
    }

    private static BoardData CreateBoardData(string path, Vector2Int size, UnitData enemyUnit, Vector2Int[] obstacles, Vector2Int[] enemySpawns)
    {
        var data = ScriptableObject.CreateInstance<BoardData>();
        data.boardSize = size;
        var tiles = new List<TileDefinition>();
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                tiles.Add(new TileDefinition
                {
                    pos = new Vector2Int(x, y),
                    type = TileType.Normal,
                    prefabOverride = null
                });
            }
        }
        foreach (var p in obstacles)
        {
            var idx = p.y * size.x + p.x;
            if (idx >= 0 && idx < tiles.Count)
            {
                var t = tiles[idx];
                t.type = TileType.Obstacle;
                tiles[idx] = t;
            }
        }
        data.tiles = tiles;

        var spawns = new List<UnitSpawnDefinition>();
        foreach (var p in enemySpawns)
        {
            spawns.Add(new UnitSpawnDefinition
            {
                pos = p,
                unit = enemyUnit,
                team = Team.Enemy
            });
        }
        data.unitSpawns = spawns;

        data.exits = new List<ExitDefinition>
        {
            new ExitDefinition{ pos = new Vector2Int(size.x-1, size.y/2), prefabOverride = null }
        };

        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    private static void BuildAndSaveScene(GameObject normal, GameObject obstacle, GameObject exitPrefab, BoardData level0, BoardData level1)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }
        cam.transform.position = new Vector3(4f, 8f, -8f);
        cam.transform.LookAt(new Vector3(4f, 0f, 2f));
        if (cam.GetComponent<PhysicsRaycaster>() == null)
            cam.gameObject.AddComponent<PhysicsRaycaster>();

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // GameRoot and managers
        var root = new GameObject("GameRoot");
        var boardMgr = root.AddComponent<BoardManager>();
        var unitMgr = root.AddComponent<UnitManager>();
        var inventory = root.AddComponent<PlayerInventory>();
        var benchMgr = root.AddComponent<BenchManager>();
        var flow = root.AddComponent<GameFlowManager>();
        root.AddComponent<AutoStartFlow>();

        // Assign references
        var boardRoot = new GameObject("BoardRoot").transform;
        boardRoot.SetParent(root.transform);
        boardMgr.boardRoot = boardRoot;
        boardMgr.normalTilePrefab = normal;
        boardMgr.obstacleTilePrefab = obstacle;
        boardMgr.exitTilePrefab = exitPrefab;

        benchMgr.benchRoot = new GameObject("BenchRoot").transform;
        benchMgr.benchRoot.SetParent(root.transform);

        flow.board = boardMgr;
        flow.unitMgr = unitMgr;
        flow.inventory = inventory;
        flow.bench = benchMgr;
        flow.levelProgression = new List<BoardData> { level0, level1 };
        flow.currentLevelIndex = 0;

        // UI Canvas + Button
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var hook = canvasGo.AddComponent<UIFlowHook>();

        var buttonGo = new GameObject("StartBattle_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(canvasGo.transform, false);
        var rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-10f, -10f);
        rect.sizeDelta = new Vector2(140f, 40f);

        var btn = buttonGo.GetComponent<Button>();
        btn.onClick.AddListener(hook.StartBattle);

        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(buttonGo.transform, false);
        var text = txtGo.GetComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.text = "Start Battle";
        text.color = Color.black;
        var txtRect = txtGo.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
    }
}

