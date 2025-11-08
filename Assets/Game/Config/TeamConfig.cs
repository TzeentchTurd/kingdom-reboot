using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "TeamConfig", menuName = "Game/Config/Team Config")]
    public class TeamConfig : ScriptableObject
    {
        [Serializable]
        public class UnitEntry
        {
            public UnitArchetype Archetype;
            [Tooltip("Index into BoardData spawn list (per side)")]
            public int SpawnIndex;
        }

        [Tooltip("Initial units for this team")] public List<UnitEntry> Units = new();
    }
}

