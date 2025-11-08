using UnityEngine;
using FlowTrace;
using Game.Battle;

namespace FlowTrace
{
    public class BattleSmokeWatchdog : MonoBehaviour
    {
        [SerializeField] private float timeoutSeconds = 30f;
        private float _startRealtime;
        private bool _ended;

        private void OnEnable()
        {
            _startRealtime = Time.realtimeSinceStartup;
            _ended = false;
        }

        private void Update()
        {
            if (_ended) return;

            if (BattleFlowTracer.ReachedResolution)
            {
                _ended = true; // normal route; end will be logged by state machine
                return;
            }

            var elapsed = Time.realtimeSinceStartup - _startRealtime;
            if (elapsed >= Mathf.Max(1f, timeoutSeconds))
            {
                _ended = true;
                BattleFlowTracer.Trace("Smoke", "timeout_30s");
                // Try to force a minimal Resolution enter/exit to keep traces coherent
                try
                {
                    var res = new ResolutionState();
                    res.Enter();
                    res.Exit();
                }
                catch {}

                BattleFlowTracer.LogBattleEnd("timeout");
            }
        }
    }
}

