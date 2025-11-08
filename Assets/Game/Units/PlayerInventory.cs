using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public List<UnitInstance> benchUnits = new List<UnitInstance>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional to persist across scenes if desired
        // DontDestroyOnLoad(gameObject);
    }

    public void AddToBench(UnitInstance inst)
    {
        benchUnits.Add(inst);
    }

    public IEnumerable<UnitInstance> TakeAllBench()
    {
        var taken = new List<UnitInstance>(benchUnits);
        benchUnits.Clear();
        return taken;
    }
}

