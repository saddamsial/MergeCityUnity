using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public class UnitManager : MonobehaviourCustom, ISingleton
    {
        protected List<Unit> units = new List<Unit>();
        protected List<UnitPoint> unitPoints = new List<UnitPoint>();
        protected List<UnitBehavior> unitBehaviors = new List<UnitBehavior>();

        public void InitAwake()
        {
        }

        public void InitStart()
        {
            Init();
        }

        public void Init()
        {
            foreach (Transform point in transform)
            {
                var unitPoint = point.GetComponent<UnitPoint>();
                unitPoints.Add(unitPoint);
            }

            foreach (var unitPoint in unitPoints)
            {
                var unit = new Unit(unitPoint.CodeName);
                var cc = GetCommandChain(unit, unitPoint.ActionsData);
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
                    GetTargetBuildingObject = GetTargetBuildingObject,
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

        private BuildObject GetTargetBuildingObject(UnitBehavior.GetTargetBuildingObjectData data)
        {
            var command = data.Command;
            string actionCodeName = command.Action.CodeName;
            string[] parts = actionCodeName.Split("_");
            string codeNameBuilding = parts[0];
            string slotType = command.Action.TypeSlot;

            return BuildingManager.Instance.PreBookBuildingAPI(codeNameBuilding, data.UnitBehavior, slotType);
        }

        protected CommandChain GetCommandChain(Unit unit, List<ActionTestData> actionsData)
        {
            var commands = new List<Command>();
            foreach (var actionData in actionsData)
            {
                var action = new Action(new Action.Data()
                {
                    ActorUnit = unit,
                    Condition = actionData.Condition,
                    CodeName = actionData.CodeName,
                });
                var movement = new Movement("Move", actionData.MoveSpeed);
                var command = new Command(unit, action, movement);
                commands.Add(command);
            }
            var commandChain = new CommandChain(commands, -1);
            return commandChain;
        }
    }
}
