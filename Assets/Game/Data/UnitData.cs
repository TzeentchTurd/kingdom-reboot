using UnityEngine;

[CreateAssetMenu(menuName = "Game/UnitData", fileName = "UnitData")]
public class UnitData : ScriptableObject
{
    public string id;
    public int HP = 1;
    public int Attack = 1;
    public float AttackSpeed = 1f;
    public float Range = 1f;
    public float MoveSpeed = 1f;
    public GameObject prefab;
    public Team defaultTeam = Team.Player;
}

