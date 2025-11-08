using UnityEngine;
using UnityEngine.EventSystems;

// Exit tile click handler to advance level progression
public class ExitTileLink : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int gridPos;

    public void OnPointerClick(PointerEventData eventData)
    {
        var flow = GameFlowManager.Instance;
        if (flow == null)
        {
            Debug.LogWarning("ExitTileLink: GameFlowManager.Instance is null.");
            return;
        }

        int levelCount = (flow.levelProgression != null) ? flow.levelProgression.Count : 0;
        if (levelCount == 0)
        {
            Debug.LogWarning("ExitTileLink: No levels configured in GameFlowManager.");
            return;
        }

        int next = flow.currentLevelIndex + 1;
        if (next >= levelCount)
        {
            // Wrap to 0 when reaching the last level (alternative per spec: log "END")
            next = 0;
        }

        flow.TransitionToLevel(next);
    }
}
