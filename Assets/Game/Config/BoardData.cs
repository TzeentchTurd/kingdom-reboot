using UnityEngine;

namespace Game.Config
{
    // Optional data provider for spawn locations on the battle board.
    // Attach to a GameObject on the BattleBoard prefab or scene.
    public class BoardData : MonoBehaviour
    {
        [Header("Spawn Points (Left Team)")]
        public Transform[] LeftSpawns;

        [Header("Spawn Points (Right Team)")]
        public Transform[] RightSpawns;
    }
}

