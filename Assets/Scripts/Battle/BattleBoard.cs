using UnityEngine;
using FlowTrace;
using Game.Flow;

namespace Game.Battle
{
    public class BattleBoard : BoardBase
    {
        // Minimal entry point; units will be injected later.
        public override void Init()
        {
            BattleFlowTracer.Trace("Board", "BattleBoard Init");
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
        }
    }
}

