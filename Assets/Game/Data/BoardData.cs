using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/BoardData", fileName = "BoardData")]
public class BoardData : ScriptableObject
{
    // Optional: can be left at (0,0) if unused
    public Vector2Int boardSize;

    public List<TileDefinition> tiles = new List<TileDefinition>();
    public List<UnitSpawnDefinition> unitSpawns = new List<UnitSpawnDefinition>();
    public List<ExitDefinition> exits = new List<ExitDefinition>();
}

