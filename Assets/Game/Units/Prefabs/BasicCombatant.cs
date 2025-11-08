using System.Linq;
using UnityEngine;

public class BasicCombatant : MonoBehaviour
{
    private UnitInstanceComponent selfComp;
    private float attackTimer;

    private void Awake()
    {
        selfComp = GetComponent<UnitInstanceComponent>();
        if (selfComp == null)
        {
            selfComp = gameObject.AddComponent<UnitInstanceComponent>();
            selfComp.inst = new UnitInstance
            {
                data = null,
                team = Team.Enemy,
                hp = 1
            };
        }
    }

    private void Update()
    {
        var flow = GameFlowManager.Instance;
        if (flow == null || flow.CurrentState != GameState.Battle)
            return;

        var unitMgr = UnitManager.Instance;
        if (unitMgr == null)
            return;

        var myTeam = selfComp.inst.team;
        var myData = selfComp.inst.data;

        float moveSpeed = myData != null ? myData.MoveSpeed : 2f;
        int attack = myData != null ? myData.Attack : 1;
        float atkSpeed = myData != null ? myData.AttackSpeed : 1f;
        float range = myData != null ? myData.Range : 1.25f;

        // find nearest enemy from tracked visuals
        GameObject targetGo = null;
        float bestDist = float.MaxValue;
        foreach (var go in unitMgr.allUnitsVisual.ToList())
        {
            if (go == null) continue;
            var comp = go.GetComponent<UnitInstanceComponent>();
            if (comp == null) continue;
            if (comp.inst.team == myTeam) continue;
            if (comp.inst.hp <= 0) continue;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                targetGo = go;
            }
        }

        if (targetGo == null) return;

        // move towards target if out of range
        if (bestDist > range)
        {
            var dir = (targetGo.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();
                transform.position += dir * moveSpeed * Time.deltaTime;
            }
            return;
        }

        // attack when in range
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = 1f / Mathf.Max(0.01f, atkSpeed);
            var targetComp = targetGo.GetComponent<UnitInstanceComponent>();
            if (targetComp != null)
            {
                var inst = targetComp.inst;
                inst.hp -= attack;
                targetComp.inst = inst;

                if (inst.hp <= 0)
                {
                    // remove and destroy
                    UnitManager.Instance.allUnitsVisual.Remove(targetGo);
                    Destroy(targetGo);
                }
            }
        }
    }
}

