using UnityEngine;

public class GameFlowUI : MonoBehaviour
{
    public GameFlowManager flow;

    private void Reset()
    {
        if (flow == null) flow = GameFlowManager.Instance;
    }

    private void OnGUI()
    {
        if (flow == null) flow = GameFlowManager.Instance;

        var buttonWidth = 140f;
        var buttonHeight = 40f;
        var margin = 10f;
        var rect = new Rect(Screen.width - buttonWidth - margin, margin, buttonWidth, buttonHeight);

        if (GUI.Button(rect, "Start Battle"))
        {
            if (flow != null)
            {
                flow.ChangeState(GameState.Battle);
            }
        }

        if (flow != null && flow.CurrentState == GameState.Victory)
        {
            var labelRect = new Rect(Screen.width - buttonWidth - margin, rect.yMax + 5f, buttonWidth, 25f);
            GUI.Label(labelRect, "Victory!");
        }
    }
}

