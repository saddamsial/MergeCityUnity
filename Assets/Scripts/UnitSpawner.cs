using Hiker.Idle;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    protected Data data;
    protected bool hasInit;

    [SerializeField]
    protected Transform spawnPoint;
    [SerializeField]
    protected Transform exitPoint;

    protected UnitSpawnerConfig config;
    protected UnityEngine.Object unitResource;
    protected List<UnitBehavior> unitBehaviors = new List<UnitBehavior>();
    protected float intervalTimer;

    public void Init(Data data)
    {
        this.data = data;
        config = ConfigManager.Instance.UnitSpawnerConfig;
        unitResource = Resources.Load("Prefabs/Unit");

        intervalTimer = config.IntervalDuration;

        hasInit = true;
    }

    private void Update()
    {
        if (!hasInit) { return; }

        var tobeRemovedUnitBehaviors = new List<UnitBehavior>();
        foreach (var unitBehavior in unitBehaviors)
        {
            var unit = unitBehavior.Unit;
            if (unit.HasExited)
            {
                tobeRemovedUnitBehaviors.Add(unitBehavior);
            }
            unit.Update(new Unit.TickData()
            {
                DeltaTime = Time.deltaTime,
            });
        }

        foreach (var unitBehavior in tobeRemovedUnitBehaviors)
        {
            Destroy(unitBehavior.gameObject);
            unitBehaviors.Remove(unitBehavior);
        }

        if (intervalTimer < config.IntervalDuration)
        {
            intervalTimer += Time.deltaTime;
        }
        else
        {
            unitBehaviors.AddRange(SpawnUnits());

            intervalTimer -= config.IntervalDuration;
        }
    }

    private List<UnitBehavior> SpawnUnits()
    {
        var spawnedUnitBehaviors = new List<UnitBehavior>();
        int count = config.IntervalMaxUnitCount;
        for (int i = 0; i < count; i++)
        {
            var spawnUnitConfig = config.GetRandomPoolItem();
            if (spawnUnitConfig == null) { continue; }

            string codeName = spawnUnitConfig.CodeName;
            var unitConfig = ConfigManager.Instance.GetUnitConfig(codeName);
            if (unitConfig == null) { continue; }

            var unit = new Unit(codeName);
            var cc = GetCommandChain(unit);
            unit.Init(new Unit.Data()
            {
                CommandChain = cc,
            });

            var go = Instantiate(unitResource, transform) as GameObject;
            go.transform.position = spawnPoint.position;
            var unitBehavior = go.GetComponent<UnitBehavior>();

            var modelResource = Resources.Load("Prefabs/" + codeName);
            var modelGO = Instantiate(modelResource) as GameObject;

            unitBehavior.Init(new UnitBehavior.Data()
            {
                Unit = unit,
                Model = modelGO,
                GetTargetBuildingObject = data.GetTargetBuilding,
                GetExitPosition = () => exitPoint.position,
            });

            unit.Start();
            spawnedUnitBehaviors.Add(unitBehavior);
        }

        return spawnedUnitBehaviors;
    }

    private CommandChain GetCommandChain(Unit unit)
    {
        var unitConfig = ConfigManager.Instance.GetUnitConfig(unit.CodeName);
        if (unitConfig == null)
        {
            return null;
        }

        var commands = new List<Command>();
        foreach (var activity in unitConfig.Routine)
        {
            var action = new Hiker.Idle.Action(new Hiker.Idle.Action.Data()
            {
                CodeName = activity.CodeName,
                ActorUnit = unit,
            });
            var movement = new Movement("Move", unitConfig.BaseMoveSpeed);
            var command = new Command(unit, action, movement);
            commands.Add(command);
        }
        var commandChain = new CommandChain(commands, unitConfig.RoutineLoopCount);
        return commandChain;
    }

    public class Data
    {
        public Func<UnitBehavior.GetTargetBuildingObjectData, BuildObject> GetTargetBuilding;
    }
}
