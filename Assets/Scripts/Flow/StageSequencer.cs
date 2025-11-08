using System;
using UnityEngine;
using FlowTrace;
using Game.Battle;

namespace Game.Flow
{
    public class StageSequencer : MonoBehaviour
    {
        [SerializeField] private Transform stageRoot;
        [SerializeField] private GameObject battleBoardPrefab;
        [SerializeField] private GameObject eventBoardPrefab;

        private GameObject _currentInstance;
        private BoardBase _currentBoard;
        private string _currentName;

        public event Action<string> stage_loaded;
        public event Action<string> stage_unloaded;
        public event Action<string, string> stage_transition; // from, to

        public Transform StageRoot
        {
            get => stageRoot;
            set => stageRoot = value;
        }

        public GameObject BattleBoardPrefab
        {
            get => battleBoardPrefab;
            set => battleBoardPrefab = value;
        }

        public GameObject EventBoardPrefab
        {
            get => eventBoardPrefab;
            set => eventBoardPrefab = value;
        }

        public void LoadBattleBoard()
        {
            LoadPrefab(battleBoardPrefab, nameof(BattleBoard));
        }

        public void LoadEventBoard()
        {
            LoadPrefab(eventBoardPrefab, nameof(EventBoard));
        }

        public void NextStage()
        {
            if (_currentName == nameof(BattleBoard))
            {
                LoadEventBoard();
            }
            else
            {
                LoadBattleBoard();
            }
        }

        public void UnloadCurrent()
        {
            if (_currentInstance != null)
            {
                try
                {
                    _currentBoard?.Exit();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (Application.isPlaying)
                    Destroy(_currentInstance);
                else
                    DestroyImmediate(_currentInstance);
                _currentInstance = null;
                var prev = _currentName;
                _currentName = null;
                stage_unloaded?.Invoke(prev);
                BattleFlowTracer.Trace("Stage", $"Unloaded {prev}");
            }
        }

        private void LoadPrefab(GameObject prefab, string nameHint)
        {
            var from = _currentName;

            if (prefab == null)
            {
                Debug.LogWarning($"StageSequencer: Prefab for {nameHint} is not assigned.");
            }

            UnloadCurrent();

            if (prefab != null)
            {
                _currentInstance = Instantiate(prefab, stageRoot);
                _currentBoard = _currentInstance.GetComponent<BoardBase>();
                if (_currentBoard == null)
                {
                    _currentBoard = _currentInstance.AddComponent<DummyBoard>();
                }
                _currentBoard.Init();
                _currentBoard.Enter();
            }
            _currentName = nameHint;

            stage_loaded?.Invoke(_currentName);
            BattleFlowTracer.Trace("Stage", $"Loaded {_currentName}");
            if (!string.IsNullOrEmpty(from))
            {
                stage_transition?.Invoke(from, _currentName);
            }
        }

        private class DummyBoard : BoardBase {}
    }
}
