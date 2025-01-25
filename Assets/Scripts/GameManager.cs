using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    protected LandAreaManager areaManager;
    //[SerializeField]
    //protected UnitManager unitManager;
    [SerializeField]
    protected UnitSpawner unitSpawner;

    public EnergyController energyController;

    private void Awake()
    {
        ConfigManager.Init();
        energyController = new EnergyController();
        energyController.Init();
    }

    private void Start()
    {
        areaManager.Init();
        //unitManager.Init(new UnitManager.Data()
        //{
        //    GetTargetBuilding = GetTargetBuilding,
        //});
        unitSpawner.Init(new UnitSpawner.Data()
        {
            GetTargetBuilding = GetTargetBuilding,
        });
     
    }

    private Hiker.Idle.BuildObject GetTargetBuilding(Hiker.Idle.UnitBehavior.GetTargetBuildingObjectData data)
    {
        var command = data.Command;
        string actionCodeName = command.Action.CodeName;
        string[] parts = actionCodeName.Split("_");
        string buildingGroupCodeName = parts[0];
        string slotType = command.Action.TypeSlot;

        return areaManager.PrebookBuilding(buildingGroupCodeName, data.UnitBehavior, slotType);
    }
}
