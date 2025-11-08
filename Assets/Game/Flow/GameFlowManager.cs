using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Levels")]
    public List<BoardData> levelProgression = new List<BoardData>();
    public int currentLevelIndex = 0;

    [Header("References")]
    public BoardManager board;
    public UnitManager unitMgr;
    public PlayerInventory inventory;
    public BenchManager bench;

    private GameState state = GameState.Preparation;
    private Coroutine battleRoutine;

    public GameState CurrentState => state;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    public void TransitionToLevel(int index)
    {
        // A) Recall player units to bench
        if (unitMgr != null)
        {
            unitMgr.RecallAllPlayerUnits();
        }

        // B) Clear the board
        if (board != null)
        {
            board.ClearBoard();
        }

        // C) Clamp and set current level index
        if (levelProgression != null && levelProgression.Count > 0)
        {
            currentLevelIndex = Mathf.Clamp(index, 0, levelProgression.Count - 1);
        }
        else
        {
            currentLevelIndex = 0;
        }

        // D) Go to Preparation
        ChangeState(GameState.Preparation);
    }

    public void ChangeState(GameState s)
    {
        // Stop any existing battle monitoring when leaving Battle
        if (battleRoutine != null)
        {
            StopCoroutine(battleRoutine);
            battleRoutine = null;
        }

        state = s;

        if (s == GameState.Preparation)
        {
            if (board != null && levelProgression != null && levelProgression.Count > 0)
            {
                var data = levelProgression[Mathf.Clamp(currentLevelIndex, 0, levelProgression.Count - 1)];
                board.BuildBoard(data);
            }
            if (bench != null)
            {
                bench.RespawnAllBenchUnits_VISUAL_ONLY();
            }
        }
        else if (s == GameState.Battle)
        {
            battleRoutine = StartCoroutine(MonitorBattle());
        }
        else if (s == GameState.Victory)
        {
            ShowVictoryScreen();
            if (board != null && levelProgression != null && levelProgression.Count > 0)
            {
                var data = levelProgression[Mathf.Clamp(currentLevelIndex, 0, levelProgression.Count - 1)];
                board.SpawnExitTiles(data);
            }
        }
    }

    private void ShowVictoryScreen()
    {
        // Minimal placeholder UI hook
        Debug.Log("Victory! Showing victory screen placeholder.");
    }

    private IEnumerator MonitorBattle()
    {
        // Poll periodically for remaining enemies
        while (true)
        {
            if (!AreAnyEnemiesAlive())
            {
                ChangeState(GameState.Victory);
                yield break;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    private bool AreAnyEnemiesAlive()
    {
        if (unitMgr == null)
        {
            unitMgr = UnitManager.Instance;
        }
        if (unitMgr == null)
        {
            return false;
        }

        // Clean up nulls
        unitMgr.allUnitsVisual.RemoveAll(g => g == null);

        foreach (var go in unitMgr.allUnitsVisual)
        {
            if (go == null) continue;
            var comp = go.GetComponent<UnitInstanceComponent>();
            if (comp == null) continue;
            if (comp.inst.team != Team.Enemy) continue;
            // consider hp > 0 and active as alive
            if (comp.inst.hp > 0 && go.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }
}

