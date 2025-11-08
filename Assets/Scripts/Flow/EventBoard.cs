using UnityEngine;
using FlowTrace;

namespace Game.Flow
{
    public class EventBoard : BoardBase
    {
        public override void Init()
        {
            BattleFlowTracer.Trace("Board", "EventBoard Init");
        }

        public override void Enter()
        {
            BattleFlowTracer.Trace("Board", "EventBoard Enter (事件棋盘)");
            gameObject.SetActive(true);
        }

        public override void Exit()
        {
            BattleFlowTracer.Trace("Board", "EventBoard Exit");
            gameObject.SetActive(false);
        }
    }
}

