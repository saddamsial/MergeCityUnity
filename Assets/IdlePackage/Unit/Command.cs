using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public class Command
    {
        public Unit Unit { get; protected set; }
        public Action Action { get; protected set; }
        public Movement Movement { get; protected set; }

        public Command(Unit unit, Action action, Movement movement)
        {
            Unit = unit;
            Action = action;
            Movement = movement;
        }

        public BuildObject BuildingObject { get; protected set; }
        public BuildSlot BuildingSlot { get; protected set; }

        public void Init(BuildObject buildingObject)
        {
            SetBuildingObject(buildingObject);
        }

        protected void HandleBuildingObjectFullSlot()
        {
            if (Unit.TryGetAvailableBuildingObject(this, out var buildingObject))
            {
                SetBuildingObject(buildingObject);
            }
        }

        protected void HandleSlotConfirmed(string unitID, BuildSlot buildSlot)
        {
            if (string.Equals(Unit.ID, unitID))
            {
                BuildingObject.OnCheckConditionCallBack += CheckStartCondition;
                BuildingObject.OutQueueUpdated -= HandleOutQueueUpdated;

                BuildingSlot = buildSlot;
                Unit.NotifySlotConfirmed(buildSlot);
            }
        }

        protected void HandleTargetQueuePositionUpdated(string typeSlot, Vector3 newQueuePosition)
        {
            if (string.Equals(typeSlot, Action.TypeSlot))
            {
                Unit.NotifyMovePositionUpdated(newQueuePosition, this);
            }
        }

        protected void HandleOutQueueUpdated(string typeSlot)
        {
            if (string.Equals(typeSlot, Action.TypeSlot))
            {
                Unit.NotifyOutQueueUpdated(this);
            }
        }

        protected void CheckStartCondition(List<string> startableUnitIDs, float duration)
        {
            if (startableUnitIDs.Contains(Unit.ID))
            {
                BuildingObject.OnCheckConditionCallBack -= CheckStartCondition;

                Unit.StartCommandAction(BuildingSlot, duration);
            }
        }

        protected void SetBuildingObject(BuildObject buildingObject)
        {
            CleanUpBuilding(BuildingObject);

            BuildingObject = buildingObject;
            BuildingObject.UpdatePreBookEvent += HandleBuildingObjectFullSlot;
            BuildingObject.OnSlotConfirmed += HandleSlotConfirmed;
            BuildingObject.UpdateTargetQueuePositionEvent += HandleTargetQueuePositionUpdated;
            Unit.NotifyTargetBuildingObjectSet(buildingObject, this);
        }

        public void MarkReached()
        {
            if (BuildingObject != null)
            {
                BuildingObject.UpdatePreBookEvent -= HandleBuildingObjectFullSlot;
                BuildingObject.UpdateTargetQueuePositionEvent -= HandleTargetQueuePositionUpdated;
            }
        }

        public void MarkJoinQueue()
        {
            if (BuildingObject != null)
            {
                BuildingObject.OutQueueUpdated += HandleOutQueueUpdated;
            }
        }

        public void CleanUp()
        {
            CleanUpBuilding(BuildingObject);
        }

        protected void CleanUpBuilding(BuildObject buildingObject)
        {
            if (buildingObject != null)
            {
                buildingObject.UpdatePreBookEvent -= HandleBuildingObjectFullSlot;
                buildingObject.OnSlotConfirmed -= HandleSlotConfirmed;
                buildingObject.UpdateTargetQueuePositionEvent -= HandleTargetQueuePositionUpdated;
                BuildingObject.OutQueueUpdated -= HandleOutQueueUpdated;
            }
        }
    }

    [System.Serializable]
    public class Action
    {
        protected Data data;
        public string CodeName => data.CodeName;
        public float Duration { get; set; }
        public string TypeSlot { get; protected set; }
        public Condition Condition => data.Condition;

        public Action(Data data)
        {
            this.data = data;

            string[] parts = CodeName.Split("_");
            string slotType = null;
            if (parts.Length > 1)
            {
                slotType = parts[1];
            }
            TypeSlot = slotType;
        }

        [System.Serializable]
        public class Data
        {
            public Unit ActorUnit;

            public string CodeName;
            public Condition Condition;
            //public float Duration;
        }
    }

    [System.Serializable]
    public class Movement
    {
        public string CodeName { get; protected set; }
        public float Speed { get; protected set; }

        public Movement(string codeName, float speed)
        {
            CodeName = codeName;
            Speed = speed;
        }
    }

    [System.Serializable]
    public class CommandChain
    {
        protected List<Command> chain;
        public int ChainCount => chain.Count;
        public int MaxLoopCount { get; protected set; }
        public bool IsLoopInfinite => MaxLoopCount < 0;

        public CommandChain(List<Command> chain, int loopTime)
        {
            this.chain = chain;

            MaxLoopCount = loopTime;
        }

        public Command GetCommand(int index)
        {
            return chain[index];
        }

        public bool IsEmpty()
        {
            return chain.Count == 0;
        }
    }

    [System.Serializable]
    public class Condition
    {
        public string TypeCondition;
        public string TypeSlot;

        public bool HasInit => !string.IsNullOrEmpty(TypeCondition);
    }
}
