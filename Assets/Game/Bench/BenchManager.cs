using System.Collections.Generic;
using UnityEngine;

public class BenchManager : MonoBehaviour
{
    public Transform benchRoot;
    public GameObject benchSlotPrefab;

    public void RespawnAllBenchUnits_VISUAL_ONLY()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("PlayerInventory.Instance not present; cannot respawn bench visuals.");
            return;
        }

        if (benchRoot == null)
        {
            Debug.LogWarning("BenchManager.benchRoot is not assigned.");
            return;
        }

        IEnumerable<UnitInstance> units = PlayerInventory.Instance.TakeAllBench();
        foreach (var u in units)
        {
            Transform parent = benchRoot;
            if (benchSlotPrefab != null)
            {
                var slot = Instantiate(benchSlotPrefab, benchRoot).transform;
                slot.name = $"BenchSlot_{(u.data != null ? u.data.name : "Unit")}";
                parent = slot;
            }

            if (u.data != null && u.data.prefab != null)
            {
                var display = Instantiate(u.data.prefab, parent);
                display.name = (u.data != null ? u.data.name : "Unit") + "_Display";
                // Ensure the display is not treated as an in-battle unit
                var comps = display.GetComponentsInChildren<UnitInstanceComponent>(true);
                foreach (var c in comps)
                {
                    if (c != null) Destroy(c);
                }
            }
        }
    }
}

