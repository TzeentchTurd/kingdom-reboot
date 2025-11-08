using System;
using UnityEngine;

// Shared data definitions used by ScriptableObjects

public enum TileType
{
    Normal,
    Obstacle
}

[Serializable]
public struct TileDefinition
{
    public Vector2Int pos;
    public TileType type;
    public GameObject prefabOverride; // nullable reference
}

public enum Team
{
    Player,
    Enemy
}

[Serializable]
public struct UnitSpawnDefinition
{
    public Vector2Int pos;
    public UnitData unit;
    public Team team;
}

[Serializable]
public struct ExitDefinition
{
    public Vector2Int pos;
    public GameObject prefabOverride; // nullable reference
}

