using UnityEngine;

public class AutoStartFlow : MonoBehaviour
{
    public int startLevelIndex = 0;

    private void Start()
    {
        var flow = GameFlowManager.Instance;
        if (flow != null)
        {
            flow.TransitionToLevel(startLevelIndex);
        }
    }
}

