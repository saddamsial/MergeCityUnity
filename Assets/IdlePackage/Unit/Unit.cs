using System;
using System.Collections.Generic;

namespace Hiker.Idle
{
    public class Unit
    {
        public string ID { get; protected set; }
        public string CodeName { get; protected set; }
        protected Data data;
        protected CommandChain commandChain => data.CommandChain;

        protected Command activeCommand;
        public Command ActiveCommand => activeCommand;
        protected int activeCommandIndex = -1;
        protected int loopCounter = 0;

        public Unit(string codeName)
        {
            CodeName = codeName;
        }

        public void Init(Data data)
        {
            ID = System.Guid.NewGuid().ToString();

            this.data = data;
        }

        public void Start()
        {
            NextCommand();
        }

        public event System.Func<Command, bool> OnDoCommand;
        public event System.Action<Command> OnDoCommandAction;
        public event System.Action OnMoveToExit;
        public void Update(TickData data)
        {
            if (hasFinished)
            {
                OnMoveToExit?.Invoke();
                return;
            }

            if (activeCommand != null)
            {
                if (actionStarted)
                {
                    if (actionTimer >= activeCommand.Action.Duration)
                    {
                        CompleteCommandAction(activeCommand);
                        NextCommand();
                    }
                    else
                    {
                        OnDoCommandAction?.Invoke(activeCommand);
                        actionTimer += data.DeltaTime;
                    }
                }
                else
                {
                    if (!reachedDestination)
                    {
                        reachedDestination = OnDoCommand(activeCommand);
                    }
                }
            }
        }

        public bool BuildingReached { get; protected set; } = false;
        public void MarkReached()
        {
            activeCommand.MarkReached();
            BuildingReached = true;
        }

        public bool QueueJoined { get; protected set; } = false;
        public void MarkJoinQueue(Command command)
        {
            if (command == activeCommand)
            {
                activeCommand.MarkJoinQueue();
                QueueJoined = true;
            }
        }

        public System.Func<Command, BuildObject> GetTargetBuildingObject;
        protected bool hasFinished = false;
        public event System.Action OnCommandChainFinished;
        public void NextCommand()
        {
            if (commandChain.IsEmpty())
            {
                return;
            }

            BuildingReached = false;
            QueueJoined = false;

            if (activeCommand != null)
            {
                activeCommand.CleanUp();
            }

            if (activeCommandIndex >= commandChain.ChainCount - 1)
            {
                if (commandChain.IsLoopInfinite
                    || loopCounter < commandChain.MaxLoopCount
                    )
                {
                    activeCommandIndex = 0;
                    loopCounter++;
                }
                else
                {
                    hasFinished = true;
                    OnCommandChainFinished?.Invoke();
                    return;
                }
            }
            else
            {
                activeCommandIndex++;
            }

            activeCommand = commandChain.GetCommand(activeCommandIndex);
            var buildingObject = GetTargetBuildingObject(activeCommand);
            StartCommand(buildingObject);
        }

        protected bool reachedDestination = false;
        public void MarkCommandContinue()
        {
            reachedDestination = false;
        }

        public event System.Action<Command> OnCommandStarted;
        protected void StartCommand(BuildObject buildingObject)
        {
            reachedDestination = false;
            activeCommand.Init(buildingObject);
            OnCommandStarted?.Invoke(activeCommand);
        }

        public event System.Action<BuildSlot> OnSlotConfirmed;
        public void NotifySlotConfirmed(BuildSlot buildSlot)
        {
            OnSlotConfirmed?.Invoke(buildSlot);
        }

        protected float actionTimer;
        protected bool actionStarted = false;
        public event System.Action<Command, BuildSlot> OnCommandActionStarted;
        public bool StartCommandAction(BuildSlot buildSlot, float duration)
        {
            if (activeCommand == null)
            {
                return false;
            }

            actionStarted = true;
            activeCommand.Action.Duration = duration;
            OnCommandActionStarted?.Invoke(activeCommand, buildSlot);
            return true;
        }

        public event System.Action<Command> OnCommandCompleted;
        protected void CompleteCommandAction(Command command)
        {
            actionStarted = false;
            actionTimer -= command.Action.Duration;
            OnCommandCompleted?.Invoke(command);
        }

        public bool TryGetAvailableBuildingObject(Command command, out BuildObject newBuildingObject)
        {
            newBuildingObject = null;
            if (command != activeCommand || actionStarted)
            {
                return false;
            }

            newBuildingObject = GetTargetBuildingObject(command);
            return true;
        }

        public event System.Action<BuildObject, Command> OnTargetBuildingObjectSet;
        public void NotifyTargetBuildingObjectSet(BuildObject buildingObject, Command command)
        {
            OnTargetBuildingObjectSet?.Invoke(buildingObject, command);
        }

        public event System.Action<UnityEngine.Vector3, Command> OnMovePositionUpdated;

        public void NotifyMovePositionUpdated(UnityEngine.Vector3 newMovePosition, Command command)
        {
            OnMovePositionUpdated?.Invoke(newMovePosition, command);
        }

        public event System.Action<Command> OnOutQueueUpdated;
        public void NotifyOutQueueUpdated(Command command)
        {
            OnOutQueueUpdated?.Invoke(command);
        }

        protected SortedDictionary<int, List<Command>> UrgentPermanentCommands = new SortedDictionary<int, List<Command>>();
        public void AddPermanentCommand(Command command, int index)
        {

        }

        public bool HasExited { get; protected set; } = false;
        public void MarkExited()
        {
            HasExited = true;
        }

        public class Data
        {
            public CommandChain CommandChain;
        }

        public class TickData
        {
            public float DeltaTime;
        }
    }
}
