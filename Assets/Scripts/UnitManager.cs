using Hiker.Idle;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    protected List<Unit> units = new List<Unit>();
    protected List<UnitPoint> unitPoints = new List<UnitPoint>();

    public void Init(Data data)
    {
        foreach (Transform point in transform)
        {
            var unitPoint = point.GetComponent<UnitPoint>();
            unitPoints.Add(unitPoint);
        }

        foreach (var unitPoint in unitPoints)
        {
            var unit = new Unit(unitPoint.CodeName);
            var cc = GetCommandChain(unit);
            unit.Init(new Unit.Data()
            {
                CommandChain = cc,
            });
            units.Add(unit);
        }

        var unitBehaviorResource = Resources.Load("Prefabs/Unit");
        for (int i = 0; i < units.Count; i++)
        {
            var go = Instantiate(unitBehaviorResource) as GameObject;
            var unitBehavior = go.GetComponent<UnitBehavior>();

            var modelResource = Resources.Load("Prefabs/" + units[i].CodeName);
            var modelGO = Instantiate(modelResource) as GameObject;

            unitBehavior.Init(new UnitBehavior.Data()
            {
                Unit = units[i],
                Model = modelGO,
                GetTargetBuildingObject = data.GetTargetBuilding,
            });
            go.transform.position = unitPoints[i].transform.position;
        }

        foreach (var unit in units)
        {
            unit.Start();
        }
        hasInit = true;
    }

    protected bool hasInit = false;
    protected void Update()
    {
        if (!hasInit) { return; }

        foreach (var unit in units)
        {
            unit.Update(new Unit.TickData()
            {
                DeltaTime = Time.deltaTime,
            });
        }
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
            var action = new Action(new Action.Data()
            {
                CodeName = activity.CodeName,
                ActorUnit = unit,
            });
            var movement = new Movement("Move", unitConfig.BaseMoveSpeed);
            var command = new Command(unit, action, movement);
            commands.Add(command);
        }
        var commandChain = new CommandChain(commands, -1);
        return commandChain;
    }

    public class Data
    {
        public System.Func<UnitBehavior.GetTargetBuildingObjectData, BuildObject> GetTargetBuilding;
    }
}
