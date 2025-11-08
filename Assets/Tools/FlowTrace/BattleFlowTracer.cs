using UnityEngine;

namespace FlowTrace
{
    public static class BattleFlowTracer
    {
        public const string CategoryState = "State";
        public const string BattleEndMarker = "BATTLE_END";

        public static void Trace(string category, string message)
        {
            Debug.Log($"[Flow][{category}] {message}");
        }

        public static void TraceStateEnter(string stateName)
        {
            Trace(CategoryState, $"Enter {stateName}");
        }

        public static void TraceStateExit(string stateName)
        {
            Trace(CategoryState, $"Exit {stateName}");
        }

        public static void LogBattleEnd()
        {
            Debug.Log(BattleEndMarker);
        }
    }
}

