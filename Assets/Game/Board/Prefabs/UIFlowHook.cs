using UnityEngine;

public class UIFlowHook : MonoBehaviour
{
    public void StartBattle()
    {
        var flow = GameFlowManager.Instance;
        if (flow != null)
        {
            flow.ChangeState(GameState.Battle);
        }
    }
}

