using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogue
{
    [CreateAssetMenu(menuName = "Game/Dialogue/Bark Bank", fileName = "BarkBank")]
    public class BarkBank : ScriptableObject
    {
        public List<string> onUnitDowned = new() {
            "{unit} is down! Hold the line!",
            "{unit} took {dmg} — stay with us!",
            "{unit} downed. Patch them up, now!"
        };

        public List<string> onExtractionComplete = new() {
            "Extraction complete. Essence secured: {essence}.",
            "Payload locked. {essence} essence accounted for.",
            "We’re clear. {essence} essence on board."
        };

        public List<string> onBattleEnd = new() {
            "Route closed. Good work.",
            "Engagement concluded. Re-org in 10.",
            "That’s a wrap. Debrief incoming."
        };
    }
}

