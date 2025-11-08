using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(fileName = "UnitArchetype", menuName = "Game/Config/Unit Archetype")]
    public class UnitArchetype : ScriptableObject
    {
        [Header("Combat Stats")]
        public float HP = 10f;
        public float Attack = 2f;
        public float AttackSpeed = 1f; // attacks per second
        public float Range = 1.5f;
        public float MoveSpeed = 3.5f;

        [Header("Visuals")]
        public Color TeamColor = Color.white;
    }
}

