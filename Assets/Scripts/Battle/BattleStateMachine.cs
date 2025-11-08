using System.Collections;
using UnityEngine;
using FlowTrace;
using Game.Flow;
using Game.Dialogue;

namespace Game.Battle
{
    public class BattleStateMachine : MonoBehaviour
    {
        [SerializeField] private StageSequencer stageSequencer;
        [SerializeField] private float placementDuration = 0.5f;
        [SerializeField] private float countdownDuration = 0.5f;
        [SerializeField] private float battleDuration = 1.0f;
        [SerializeField] private float resolutionDuration = 0.5f;

        private void Reset()
        {
            if (stageSequencer == null)
            {
                stageSequencer = FindObjectOfType<StageSequencer>();
            }
        }

        private void Start()
        {
            StartCoroutine(RunFlow());
        }

        private IEnumerator RunFlow()
        {
            var placement = new PlacementState();
            yield return RunState(placement, placementDuration);

            var countdown = new CountdownState();
            yield return RunState(countdown, countdownDuration);

            var battle = new BattleBoardState(stageSequencer);
            yield return RunState(battle, battleDuration);

            var resolution = new ResolutionState();
            yield return RunState(resolution, resolutionDuration);

            BarkPlayer.Play("onBattleEnd", new BarkContext());
            BattleFlowTracer.LogBattleEndOk();
        }

        private static IEnumerator RunState(IBattleState state, float duration)
        {
            state.Enter();
            float t = 0f;
            while (t < duration)
            {
                state.Tick(Time.deltaTime);
                t += Time.deltaTime;
                yield return null;
            }
            state.Exit();
        }
    }

    public interface IBattleState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

    public class PlacementState : IBattleState
    {
        public void Enter() => BattleFlowTracer.TraceStateEnter(nameof(PlacementState));
        public void Tick(float deltaTime) {}
        public void Exit() => BattleFlowTracer.TraceStateExit(nameof(PlacementState));
    }

    public class CountdownState : IBattleState
    {
        public void Enter() => BattleFlowTracer.TraceStateEnter(nameof(CountdownState));
        public void Tick(float deltaTime) {}
        public void Exit() => BattleFlowTracer.TraceStateExit(nameof(CountdownState));
    }

    public class BattleBoardState : IBattleState
    {
        private readonly StageSequencer _sequencer;

        public BattleBoardState(StageSequencer sequencer)
        {
            _sequencer = sequencer;
        }

        public void Enter()
        {
            BattleFlowTracer.TraceStateEnter(nameof(BattleBoardState));
            _sequencer?.LoadBattleBoard();
        }

        public void Tick(float deltaTime) {}

        public void Exit()
        {
            BattleFlowTracer.TraceStateExit(nameof(BattleBoardState));
            _sequencer?.UnloadCurrent();
        }
    }

    public class ResolutionState : IBattleState
    {
        public void Enter() => BattleFlowTracer.TraceStateEnter(nameof(ResolutionState));
        public void Tick(float deltaTime) {}
        public void Exit() => BattleFlowTracer.TraceStateExit(nameof(ResolutionState));
    }
}
