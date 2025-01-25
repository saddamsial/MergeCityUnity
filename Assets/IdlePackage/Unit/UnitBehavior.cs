using UnityEngine;

namespace Hiker.Idle
{
    public class UnitBehavior : MonoBehaviour
    {
        protected Data data;
        public Unit Unit => data.Unit;
        public string ID;

        //protected Animator animator;
        protected UnitMovementBehavior movement;
        [SerializeField]
        protected Renderer cube;

        [SerializeField]
        protected Transform visualContainer;

        public void Init(Data data)
        {
            this.data = data;

            ID = data.Unit.ID;

            foreach (Transform child in visualContainer.transform)
            {
                Destroy(child.gameObject);
            }
            data.Model.transform.SetParent(visualContainer);
            data.Model.transform.localPosition = Vector3.zero;
            data.Model.transform.localRotation = Quaternion.identity;

            //animator = data.Model.GetComponent<Animator>();
            movement = GetComponent<UnitMovementBehavior>();
            movement.Init();

            Unit.OnCommandStarted += HandleStartCommand;
            Unit.OnCommandActionStarted += HandleStartCommandAction;
            Unit.OnDoCommand += HandleDoCommand;
            Unit.OnDoCommandAction += HandleDoCommandAction;
            Unit.OnCommandCompleted += HandleCommandCompleted;
            Unit.OnTargetBuildingObjectSet += HandleTargetBuildingObjectSet;
            Unit.OnSlotConfirmed += HandleSlotConfirmed;
            Unit.OnMovePositionUpdated += HandleMovePositionUpdated;
            Unit.OnOutQueueUpdated += HandleOutQueueUpdated;
            Unit.OnMoveToExit += HandleMoveToExit;
            Unit.OnCommandChainFinished += HandleCommandChainFinished;

            Unit.GetTargetBuildingObject = GetTargetBuildingObject;
        }

        private BuildObject GetTargetBuildingObject(Command command)
        {
            return data.GetTargetBuildingObject(new GetTargetBuildingObjectData()
            {
                UnitBehavior = this,
                Command = command,
            });
        }

        protected void HandleStartCommand(Command command)
        {
            // Testing
            movement.SetSpeed(command.Movement.Speed);
        }

        protected bool HandleDoCommand(Command command)
        {
            bool reached = movement.ReachedDestination;
            if (reached)
            {
                //animator.CrossFade("Idle", 0.1f);

                if (!Unit.BuildingReached)
                {
                    Unit.MarkReached();
                }

                if (!Unit.QueueJoined)
                {
                    var slotType = command.Action.TypeSlot;
                    var queueJoinResult = command.BuildingObject.JoinQueue(this, slotType);
                    MonobehaviourCustom.DebugLog(name, queueJoinResult.ToString());
                    switch (queueJoinResult)
                    {
                        case TypeJoinQueue.Queued:
                            {
                                cube.material.color = Color.yellow;

                                Unit.MarkJoinQueue(command);
                                break;
                            }
                    }
                }
            }
            return reached;
        }

        protected void HandleSlotConfirmed(BuildSlot buildSlot)
        {
            cube.material.color = Color.green;

            transform.position = buildSlot.ObjSlot.transform.position;
            transform.forward = buildSlot.ObjSlot.transform.forward;
            movement.ToggleActive(false);
            //animator.CrossFade("Idle", 0.1f);
        }

        protected void HandleStartCommandAction(Command command, BuildSlot buildSlot)
        {
            //transform.position = buildSlot.ObjSlot.transform.position;
            //transform.forward = buildSlot.ObjSlot.transform.forward;
            //movement.ToggleActive(false);

            // Testing
            //animator.CrossFade("Action_" + command.Action.CodeName, 0.1f);
        }

        protected void HandleDoCommandAction(Command command)
        {
        }

        private void HandleCommandCompleted(Command command)
        {
            transform.position = command.BuildingObject.OutPosition;
            movement.ToggleActive(true);

            var slotType = command.Action.TypeSlot;
            command.BuildingObject.ForwardSlot(Unit.ID, slotType);
        }

        protected void HandleTargetBuildingObjectSet(BuildObject buildingObject, Command command)
        {
            //Vector3 movePosition = buildingObject.InPosition;
            //movement.MoveTo(movePosition);

            cube.material.color = Color.black;

            string typeSlot = command.Action.TypeSlot;
            Vector3 movePosition = buildingObject.GetQueueCurrentPosition(typeSlot);
            movement.MoveTo(movePosition);

            //animator.CrossFade("Move_" + command.Action.CodeName, 0.1f);
        }

        protected void HandleMovePositionUpdated(Vector3 newMovePosition, Command command)
        {
            movement.MoveTo(newMovePosition);
            Unit.MarkCommandContinue();

            //animator.CrossFade("Move_" + command.Action.CodeName, 0.1f);
        }

        private void HandleOutQueueUpdated(Command command)
        {
            var newQueueMovePosition = command.BuildingObject.GetQueueCurrentPosition(command.Action.TypeSlot, ID);
            movement.MoveTo(newQueueMovePosition);
            Unit.MarkCommandContinue();

            //animator.CrossFade("Move_" + command.Action.CodeName, 0.1f);
        }

        private void HandleMoveToExit()
        {
            bool reached = movement.ReachedDestination;
            if (reached)
            {
                Unit.MarkExited();
            }
        }

        private void HandleCommandChainFinished()
        {
            Vector3 exitPosition = data.GetExitPosition();
            movement.MoveTo(exitPosition);
        }

        public class Data
        {
            public Unit Unit;
            public GameObject Model;

            public System.Func<GetTargetBuildingObjectData, BuildObject> GetTargetBuildingObject;
            public System.Func<Vector3> GetExitPosition;
        }

        public class GetTargetBuildingObjectData
        {
            public Command Command;
            public UnitBehavior UnitBehavior;
        }
    }
}
