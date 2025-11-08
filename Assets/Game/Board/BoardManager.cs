using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    public Transform boardRoot;
    public GameObject normalTilePrefab;
    public GameObject obstacleTilePrefab;
    public GameObject exitTilePrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    public void ClearBoard()
    {
        if (boardRoot == null)
        {
            Debug.LogWarning("BoardManager.ClearBoard: boardRoot is not assigned.");
        }
        else
        {
            var children = new List<Transform>();
            foreach (Transform child in boardRoot)
                children.Add(child);

            foreach (var t in children)
            {
                if (t == null) continue;
                if (Application.isPlaying)
                    Destroy(t.gameObject);
                else
                    DestroyImmediate(t.gameObject);
            }
        }

        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.allUnitsVisual.Clear();
        }
    }

    public void BuildBoard(BoardData data)
    {
        if (data == null)
        {
            Debug.LogWarning("BoardManager.BuildBoard: data is null.
");
            return;
        }

        if (boardRoot == null)
        {
            Debug.LogWarning("BoardManager.BuildBoard: boardRoot is not assigned.");
            return;
        }

        // Build tiles
        foreach (var tile in data.tiles)
        {
            GameObject prefab = tile.prefabOverride;
            if (prefab == null)
            {
                switch (tile.type)
                {
                    case TileType.Obstacle:
                        prefab = obstacleTilePrefab;
                        break;
                    default:
                        prefab = normalTilePrefab;
                        break;
                }
            }

            if (prefab == null)
            {
                Debug.LogWarning($"No prefab for tile at {tile.pos} of type {tile.type}");
                continue;
            }

            var tileGO = Instantiate(prefab, boardRoot);
            tileGO.name = $"Tile_{tile.pos.x}_{tile.pos.y}_{tile.type}";
            tileGO.transform.position = GridToWorld(tile.pos);
        }

        // Spawn enemies
        foreach (var spawn in data.unitSpawns)
        {
            if (spawn.unit == null || spawn.unit.prefab == null)
            {
                Debug.LogWarning($"Invalid unit spawn at {spawn.pos} (missing UnitData or prefab)");
                continue;
            }

            var go = Instantiate(spawn.unit.prefab, boardRoot);
            go.name = $"Unit_{spawn.unit.name}_{spawn.team}_{spawn.pos.x}_{spawn.pos.y}";
            go.transform.position = GridToWorld(spawn.pos);

            var comp = go.GetComponent<UnitInstanceComponent>();
            if (comp == null)
                comp = go.AddComponent<UnitInstanceComponent>();

            var inst = new UnitInstance
            {
                data = spawn.unit,
                team = spawn.team,
                hp = (spawn.unit != null ? spawn.unit.HP : 1)
            };
            comp.inst = inst;

            if (UnitManager.Instance != null)
            {
                UnitManager.Instance.allUnitsVisual.Add(go);
            }
        }
    }

    public void SpawnExitTiles(BoardData data)
    {
        if (data == null)
        {
            Debug.LogWarning("BoardManager.SpawnExitTiles: data is null.");
            return;
        }

        if (boardRoot == null)
        {
            Debug.LogWarning("BoardManager.SpawnExitTiles: boardRoot is not assigned.");
            return;
        }

        foreach (var exit in data.exits)
        {
            GameObject prefab = exit.prefabOverride != null ? exit.prefabOverride : exitTilePrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"No exit prefab for exit at {exit.pos}");
                continue;
            }

            var go = Instantiate(prefab, boardRoot);
            go.name = $"Exit_{exit.pos.x}_{exit.pos.y}";
            go.transform.position = GridToWorld(exit.pos);

            var link = go.GetComponent<ExitTileLink>();
            if (link == null) link = go.AddComponent<ExitTileLink>();
            link.gridPos = exit.pos;
        }
    }

    public static Vector3 GridToWorld(Vector2Int p, float cell = 1f)
    {
        return new Vector3(p.x * cell, 0f, p.y * cell);
    }
}

