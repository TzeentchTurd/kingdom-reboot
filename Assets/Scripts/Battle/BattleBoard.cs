using System.Collections.Generic;
using UnityEngine;
using FlowTrace;
using Game.Flow;
using Game.Config;

namespace Game.Battle
{
    public class BattleBoard : BoardBase
    {
        [Header("Configs")]
        [SerializeField] private TeamConfig leftTeam;
        [SerializeField] private TeamConfig rightTeam;
        [SerializeField] private BoardData boardData; // optional

        private readonly List<UnitRuntime> _leftUnits = new();
        private readonly List<UnitRuntime> _rightUnits = new();

        private class UnitRuntime
        {
            public GameObject go;
            public Transform tf;
            public UnitArchetype arch;
            public float hp;
            public float atkCd;
            public bool downed;
            public int team; // 0 = left, 1 = right
        }

        // Minimal entry point; units will be injected later.
        public override void Init()
        {
            BattleFlowTracer.Trace("Board", "BattleBoard Init");

            if (boardData == null)
                boardData = GetComponentInChildren<BoardData>();

#if UNITY_EDITOR
            // Lazy-load default team assets for convenience in Editor if not wired.
            if (leftTeam == null)
            {
                leftTeam = UnityEditor.AssetDatabase.LoadAssetAtPath<TeamConfig>(
                    "Assets/Game/Config/Teams/LeftTeam.asset");
            }
            if (rightTeam == null)
            {
                rightTeam = UnityEditor.AssetDatabase.LoadAssetAtPath<TeamConfig>(
                    "Assets/Game/Config/Teams/RightTeam.asset");
            }
#endif

            SpawnTeam(0, leftTeam, _leftUnits);
            SpawnTeam(1, rightTeam, _rightUnits);
        }

        public override void Enter()
        {
            BattleFlowTracer.Trace("Board", "BattleBoard Enter");
            gameObject.SetActive(true);
        }

        public override void Exit()
        {
            BattleFlowTracer.Trace("Board", "BattleBoard Exit");
            gameObject.SetActive(false);

            // Cleanup spawned units
            foreach (var u in _leftUnits)
                if (u?.go) DestroySafe(u.go);
            foreach (var u in _rightUnits)
                if (u?.go) DestroySafe(u.go);
            _leftUnits.Clear();
            _rightUnits.Clear();
        }

        private void Update()
        {
            // Simple loop: approach → attack when in range
            StepUnits(_leftUnits, _rightUnits);
            StepUnits(_rightUnits, _leftUnits);
        }

        private void StepUnits(List<UnitRuntime> allies, List<UnitRuntime> enemies)
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < allies.Count; i++)
            {
                var u = allies[i];
                if (u == null || u.downed || u.tf == null) continue;

                // find nearest alive enemy
                UnitRuntime target = null;
                float best = float.MaxValue;
                for (int j = 0; j < enemies.Count; j++)
                {
                    var e = enemies[j];
                    if (e == null || e.downed || e.tf == null) continue;
                    float d = Vector3.Distance(u.tf.position, e.tf.position);
                    if (d < best)
                    {
                        best = d;
                        target = e;
                    }
                }

                if (target == null) continue;

                // move or attack
                if (best > u.arch.Range)
                {
                    Vector3 dir = (target.tf.position - u.tf.position).normalized;
                    u.tf.position += dir * (u.arch.MoveSpeed * dt);
                }
                else
                {
                    u.atkCd -= dt;
                    if (u.atkCd <= 0f)
                    {
                        u.atkCd = Mathf.Max(0.01f, 1f / Mathf.Max(0.01f, u.arch.AttackSpeed));
                        ApplyDamage(u, target, u.arch.Attack);
                    }
                }
            }
        }

        private void ApplyDamage(UnitRuntime src, UnitRuntime tgt, float dmg)
        {
            if (tgt.downed) return;
            tgt.hp -= dmg;
            if (tgt.hp <= 0f)
            {
                tgt.downed = true;
                if (tgt.go != null)
                {
                    var rend = tgt.go.GetComponentInChildren<Renderer>();
                    if (rend != null)
                    {
                        rend.enabled = false;
                    }
                }
                BattleFlowTracer.Trace("Downed", $"Team{tgt.team} {tgt.arch?.name ?? "Unit"} downed");
            }
        }

        private void SpawnTeam(int teamIndex, TeamConfig cfg, List<UnitRuntime> bucket)
        {
            if (cfg == null) return;
            var entries = cfg.Units;
            if (entries == null) return;

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null || e.Archetype == null) continue;
                Vector3 pos = GetSpawnPos(teamIndex, e.SpawnIndex, i);
                var u = CreateUnitGO(teamIndex, e.Archetype, pos);
                bucket.Add(u);
            }
        }

        private Vector3 GetSpawnPos(int teamIndex, int spawnIndex, int fallbackIndex)
        {
            Transform t = null;
            if (boardData != null)
            {
                var arr = teamIndex == 0 ? boardData.LeftSpawns : boardData.RightSpawns;
                if (arr != null && spawnIndex >= 0 && spawnIndex < arr.Length)
                {
                    t = arr[spawnIndex];
                }
            }

            if (t != null) return t.position;

            // Fallback: generate simple offsets
            float x = teamIndex == 0 ? -5f : 5f;
            float z = fallbackIndex * 2f;
            return new Vector3(x, 0f, z);
        }

        private UnitRuntime CreateUnitGO(int teamIndex, UnitArchetype arch, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Unit_{(teamIndex == 0 ? "L" : "R")}_{arch.name}";
            go.transform.SetParent(transform, false);
            go.transform.position = pos;

            var rend = go.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                // unique material instance for color tint
                var mat = new Material(rend.sharedMaterial);
                mat.color = arch.TeamColor;
                rend.material = mat;
            }

            return new UnitRuntime
            {
                go = go,
                tf = go.transform,
                arch = arch,
                hp = arch.HP,
                atkCd = 0f,
                downed = false,
                team = teamIndex
            };
        }

        private static void DestroySafe(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go); else DestroyImmediate(go);
        }
    }
}
