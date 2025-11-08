using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    public List<GameObject> allUnitsVisual = new List<GameObject>();

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

    public void RecallAllPlayerUnits()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("PlayerInventory.Instance not present; cannot recall units.");
            return;
        }

        var comps = Resources.FindObjectsOfTypeAll<UnitInstanceComponent>();
        var toRemoveFromVisual = new List<GameObject>();

        foreach (var comp in comps)
        {
            if (comp == null) continue;

            // Only consider instances that are actually in a loaded scene (exclude assets/prefabs)
            if (!comp.gameObject.scene.IsValid()) continue;

            var unit = comp.inst;
            if (unit.data == null) continue;
            if (unit.team != Team.Player) continue;

            PlayerInventory.Instance.AddToBench(unit);

            if (allUnitsVisual.Contains(comp.gameObject))
            {
                toRemoveFromVisual.Add(comp.gameObject);
            }

            Destroy(comp.gameObject);
        }

        // Remove from tracking list and clean nulls
        foreach (var go in toRemoveFromVisual)
        {
            allUnitsVisual.Remove(go);
        }
        allUnitsVisual.RemoveAll(g => g == null);
    }
}

