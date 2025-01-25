using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hiker.Idle
{
    public class BuildObject : MonobehaviourCustom
    {
        public string CodeNameBuilding;
        public string CodeName;
        public GameObject InPointBuilding;
        public GameObject OutPointBuilding;
        public List<ActivitySlot> activitySlots;
        public List<ActivitySlot> ActivitySlots => activitySlots;
        //public List<SlotObject> ObjBuildSlot = new List<SlotObject>();
        public List<DataCheckCondition> dataCheckConditions = new List<DataCheckCondition>();

        //public Vector3 InPosition => InPointBuilding.transform.position;
        //public Vector3 OutPosition => OutPointBuilding.transform.position;
        public Vector3 InPosition { get; protected set; }
        public Vector3 OutPosition { get; protected set; }
        public System.Action<BuildObject> UpdateBuildingBaseEvent;

        int CurrentInitSlot = 1;

        #region Function Init
        public void Init(List<ActivityData> activityDataList, Vector3 inPosition, Vector3 outPosition)
        {
            activitySlots = new List<ActivitySlot>();

            foreach (var activityData in activityDataList)
            {
                activitySlots.Add(new ActivitySlot()
                {
                    typeSlot = activityData.CodeName,
                    CodeName = activityData.CodeName,
                    queueSlots = activityData.QueuePoints.Select(
                        item => new QueueSlot()
                        {
                            IdUnit = null,
                            ObjSlot = item.gameObject,
                        }).ToList(),
                    buildSlots = activityData.ActivityPoints.Select(
                        item => new BuildSlot()
                        {
                            IdUnit = null,
                            ObjSlot = item.gameObject,
                        }).ToList(),
                    Duration = activityData.Duration,
                });
            }

            InPosition = inPosition;
            OutPosition = outPosition;
        }

        [System.Obsolete("Move to \"Init(...)\"")]
        public bool InitBuildSlots(List<SlotObject> objects)
        {
            //ObjBuildSlot.Clear();
            if (objects == null || objects.Count <= 0)
            {
                var lst = GetComponentsInChildren<SlotObject>();
                objects = ConverToListFromArray(lst);
                //ObjBuildSlot = objects;
            }

            if (objects != null && objects.Count > 0)
            {
                activitySlots.Clear();
                activitySlots = new List<ActivitySlot>();

                CurrentInitSlot = 1;
                foreach (var item in objects)
                {
                    AddBuildSlotToActivitySlot(item);
                }
            }

            //Queue Slot Init
            var temp = GetComponentsInChildren<SlotQueueObject>();
            List<SlotQueueObject> queueSlots = ConverToListFromArray(temp);
            if(queueSlots != null && queueSlots.Count > 0)
            {
                foreach (var item in queueSlots)
                {
                    foreach (var itemActivity in activitySlots)
                    {
                        if(item.TypeQueueSlot == itemActivity.typeSlot)
                        {
                            itemActivity.queueSlots.Add(new QueueSlot()
                            {
                                IdUnit = null,
                                ObjSlot = item.gameObject,
                            });
                            break;
                        }
                    }
                }
            }

            return true;
        }

        public ActivitySlot GetActivitySlot(string typeSlot)
        {
            if (typeSlot == null && activitySlots != null && activitySlots.Count >= 1) return activitySlots[0];

            foreach (var item in activitySlots)
            {
                if (item.typeSlot == typeSlot)
                {
                    return item;
                }
            }
            return null;
        }

        public void AddBuildSlotToActivitySlot(SlotObject slotObject)
        {
            string keySlot = slotObject.GetTypeSlot();
            ActivitySlot activitySlot = GetActivitySlot(keySlot);
            if (activitySlot == null)
            {
                activitySlot = new ActivitySlot()
                {
                    CodeName = this.CodeName + "_" + CurrentInitSlot++,
                    typeSlot = keySlot,
                    buildSlots = new List<BuildSlot>(),
                    QueueObjs = new List<UnitBehavior>(),
                    queueSlots = new List<QueueSlot>(),
                };

                activitySlots.Add(activitySlot);
            }
            BuildSlot buildSlot = new BuildSlot()
            {
                IdUnit = null,
                ObjSlot = slotObject.gameObject,
            };
            activitySlot.buildSlots.Add(buildSlot);
        }

        public void SetPositionBuildObject(Vector3 Pos)
        {
            transform.position = Pos;
        }

        #endregion

        #region Function BuildObject
        public bool IsHasSlot()
        {
            foreach (var item in activitySlots)
            {
                foreach (var item2 in item.buildSlots)
                {
                    if (string.IsNullOrEmpty(item2.IdUnit))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsHasSlot(string typeSlot)
        {
            foreach (var item in activitySlots)
            {
                if (item.typeSlot == typeSlot)
                {
                    foreach (var item2 in item.buildSlots)
                    {
                        if (string.IsNullOrEmpty(item2.IdUnit))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public BuildSlot GetBuildSlot()
        {
            foreach (var item in activitySlots)
            {
                foreach (var i2 in item.buildSlots)
                {
                    if (string.IsNullOrEmpty(i2.IdUnit))
                    {
                        return i2;
                    }
                }
            }
            return null;
        }

        public BuildSlot GetBuildSlot(string typeSlot)
        {
            foreach (var item in activitySlots)
            {
                if (item.typeSlot == typeSlot)
                {
                    foreach (var i2 in item.buildSlots)
                    {
                        if (string.IsNullOrEmpty(i2.IdUnit))
                        {
                            return i2;
                        }
                    }
                }
            }
            return null;
        }

        public BuildSlot GetBuildSlot(List<BuildSlot> buildSlots)
        {
            foreach (var item in buildSlots)
            {
                if (string.IsNullOrEmpty(item.IdUnit))
                {
                    return item;
                }
            }
            return null;
        }

        public List<BuildSlot> GetOccupiedBuildSlot()
        {
            var buildSlots = new List<BuildSlot>();
            foreach (var activity in activitySlots)
            {
                foreach (var buildSlot in activity.buildSlots)
                {
                    if (string.IsNullOrEmpty(buildSlot.IdUnit)) { continue; }

                    buildSlots.Add(buildSlot);
                }
            }

            return buildSlots;
        }

        public event System.Action<string, BuildSlot> OnSlotConfirmed;
        public bool ComfirmSlot(Unit unit, ActivitySlot activitySlot, string typeSlot = null)
        {
            string IdUnit = unit.ID;
            var slot = string.IsNullOrEmpty(typeSlot) ? GetBuildSlot() : GetBuildSlot(typeSlot);

            if (slot == null)
            {
                DebugLog(name, "Slot is Full", "Red", IdUnit + ": ComfirmSlot");
                return false;
            }

            //Register Slot
            slot.IdUnit = IdUnit;
            //To Do UnPrebook

            DebugLog(name, IdUnit + ": Give a Wave Slot Success", "Green", "ComfirmSlot");

            if (!(typeSlot == null ? IsHasSlot() : IsHasSlot(typeSlot)))
            {
                UpdatePreBookEvent?.Invoke();
            }

            OnSlotConfirmed?.Invoke(IdUnit, slot);

            //var startableUnitIDs = new List<string>();
            //var occupiedBuildSlots = GetOccupiedBuildSlot();
            //foreach (var buildSlot in occupiedBuildSlots)
            //{
            //    startableUnitIDs.Add(buildSlot.IdUnit);
            //}
            //DebugLog(CodeName + ": OnCheckCondition Handlers Count", OnCheckCondition.GetInvocationList().Length.ToString(), "Blue", "Log");
            //OnCheckCondition?.Invoke(startableUnitIDs);
            if (unit.ActiveCommand != null)
            {
                DebugLog(CodeName + ": OnCheckConditionCallBack Handlers Count", OnCheckConditionCallBack.GetInvocationList().Length.ToString(), "Blue", "Log");
                RegisterCondition(IdUnit, activitySlot, unit.ActiveCommand.Action.Condition);
            }

            return true;
        }

        //private void FreeSlot(string IdUnit)
        //{
        //    foreach (var item in activitySlots)
        //    {
        //        foreach (var t2 in item.buildSlots)
        //        {
        //            if (t2.IdUnit == IdUnit)
        //            { 
        //                t2.IdUnit = null;
        //                UpdateBuildingBaseEvent?.Invoke(this);
        //                DebugLog(name, IdUnit + ": Free Slot is Success", "Green", "ComfirmSlot");
        //                return;
        //            }
        //        }
        //    }
        //}

        private void FreeSlot(string IdUnit, string typeSlot = null)
        {
            //if(typeSlot == null)
            //{
            //    FreeSlot(IdUnit);
            //}
            //else
            //{
            foreach (var item in activitySlots)
            {
                if (typeSlot == null || item.typeSlot == typeSlot)
                {
                    foreach (var t2 in item.buildSlots)
                    {
                        if (t2.IdUnit == IdUnit)
                        {
                            t2.IdUnit = null;
                            UpdateBuildingBaseEvent?.Invoke(this);
                            DebugLog(name, IdUnit + ": Free Slot is Success", "Green", "ComfirmSlot");
                            return;
                        }
                    }
                }
            }
            //}
        }

        #endregion

        #region Queue Slot

        public TypeJoinQueue JoinQueue(UnitBehavior unitBehavior, string typeSlot = null)
        {
            ActivitySlot activitySlot = GetActivitySlot(typeSlot);
            if (activitySlot == null)
            {
                DebugLog(name, "ActivitySlot Not Found: " + typeSlot, "Red", "Error");
                return TypeJoinQueue.Failed;
            }

            if (IsInQueue(typeSlot, unitBehavior.ID))
            {
                return TypeJoinQueue.Failed;
            }

            BuildSlot buildSlot = GetBuildSlot(activitySlot.buildSlots);
            if (buildSlot == null)
            {
                //join Queue
                activitySlot.QueueObjs.Add(unitBehavior);

                if(activitySlot.QueueObjs.Count <= activitySlot.queueSlots.Count)
                {
                    activitySlot.queueSlots[activitySlot.QueueObjs.Count - 1].IdUnit = unitBehavior.ID;
                }

                Vector3 PositionTargetNew = GetQueueCurrentPosition(typeSlot);
                UpdateTargetQueuePositionEvent?.Invoke(typeSlot, PositionTargetNew);

                return TypeJoinQueue.Queued;
            }
            else
            {
                //Remove Event Update Pre Book
                //UnRegisterPreBookEvent(BuildingManager.ToDoEvent);

                //Pick Slot
                ComfirmSlot(unitBehavior.Unit, activitySlot, typeSlot);
                return TypeJoinQueue.Confirmed;
            }
        }

        public void OutQueue(string typeSlot = null)
        {
            ActivitySlot activitySlot = GetActivitySlot(typeSlot);
            if (activitySlot == null)
            {
                DebugLog(name, "ActivitySlot Not Found: " + typeSlot, "Red", "Error");
                return;
            }

            UnitBehavior unitBehavior = null;
            if (activitySlot.QueueObjs != null && activitySlot.QueueObjs.Count > 0)
            {
                unitBehavior = activitySlot.QueueObjs[0];
                activitySlot.QueueObjs.RemoveAt(0);
                activitySlot.queueSlots[0].IdUnit = null;

                //Don Slot Len
                NextQueue(activitySlot);
            }

            //Remove Event Update Pre Book
            //UnRegisterPreBookEvent(BuildingManager.ToDoEvent);

            if (unitBehavior == null)
            {
                DebugLog(name, "Queue empty", "White", "Queue Log");
                return;
            }

            //Pick Slot
            ComfirmSlot(unitBehavior.Unit, activitySlot, typeSlot);

            OutQueueUpdated?.Invoke(typeSlot);
            var newQueuePositionCurrent = GetQueueCurrentPosition(typeSlot);
            UpdateTargetQueuePositionEvent?.Invoke(typeSlot, newQueuePositionCurrent);
        }

        public void ForwardSlot(string unitID, string typeSlot = null)
        {
            FreeSlot(unitID, typeSlot);
            OutQueue(typeSlot);
        }

        #endregion

        #region Function Check Condition

        public bool IsStartActivity(int Count = 1, string typeSlot = null)
        {
            ActivitySlot activitySlot = GetActivitySlot(typeSlot);

            if (activitySlot.GetCountSlotActive() >= Count)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Register Event

        public System.Action UpdatePreBookEvent;
        //public void RegisterPreBookEvent(System.Action actionBookPre)
        //{
        //    UpdatePreBookEvent += actionBookPre;
        //}

        //public void UnRegisterPreBookEvent(System.Action actionBookPre)
        //{
        //    UpdatePreBookEvent -= actionBookPre;
        //}

        #endregion

        #region Reset ALL Slot is Null

        public bool ResetAllSlotIsNull(List<SlotObject> slotObjects)
        {
            if (slotObjects != null && slotObjects.Count > 0)
            {
                foreach (var item in slotObjects)
                {
                    item.ResetSlotIsNULL();
                }
            }
            return true;
        }

        #endregion

        #region Function Is Check Condition Start Action


        public System.Action<List<string>, float> OnCheckConditionCallBack;

        public bool IsExistsCondition(string IdUnit)
        {
            foreach (var item in dataCheckConditions)
            {
                if (item.IdUnit == IdUnit)
                {
                    return true;
                }
            }
            return false;
        }

        public bool AddCondition(string idUnit, Condition condition, ActivitySlot activitySlot)
        {
            if (!IsExistsCondition(idUnit))
            {
                int index = 0;
                foreach (var item in activitySlot.buildSlots)
                {
                    if (item.IdUnit == idUnit)
                    {
                        break;
                    }
                    index++;
                }

                dataCheckConditions.Add(new DataCheckCondition()
                {
                    IdUnit = idUnit,
                    Condition = condition,
                    ActivitySlot = activitySlot,
                    Index = index,
                });

                DebugLog(name, "Add Condition Success: " + idUnit, "Green", "Add Condition");
                return true;
            }

            DebugLog(name, "Exists Condition: " + idUnit, "Red", "Error Condition");
            return false;
        }


        public bool RemoveCondition(string idUnit)
        {
            if (IsExistsCondition(idUnit))
            {
                foreach (var item in dataCheckConditions)
                {
                    if (item.IdUnit == idUnit)
                    {
                        dataCheckConditions.Remove(item);
                        DebugLog(name, "Remove Condition Success: " + idUnit, "Green", "Remove Condition");
                        return true;
                    }
                }
            }

            DebugLog(name, "Not Exists Condition: " + idUnit, "Red", "Error Condition");
            return false;
        }

        public void RegisterCondition(string idUnit, ActivitySlot activitySlot, Condition condition = null)
        {
            //Add and Check
            if (!AddCondition(idUnit, condition, activitySlot))
            {
                return;
            }

            //Check
            List<string> resultIdUnitAction = IsCheckCondition();
            if (resultIdUnitAction != null && resultIdUnitAction.Count > 0)
            {
                foreach (var unitIDs in resultIdUnitAction)
                {
                    RemoveCondition(unitIDs);
                }
                OnCheckConditionCallBack.Invoke(resultIdUnitAction, activitySlot.Duration);
            }
        }

        public List<string> IsCheckCondition()
        {
            List<string> resultUnitId = new List<string>();

            foreach (var item in dataCheckConditions)
            {
                if (item.Condition == null || !item.Condition.HasInit)
                {
                    resultUnitId.Add(item.IdUnit);
                    continue;
                }
                if (item.Condition != null && item.Condition.TypeCondition == "Symmetry")
                {
                    if (ConditionSymmetry(item, dataCheckConditions))
                    {
                        resultUnitId.Add(item.IdUnit);
                    }
                    continue;
                }
            }


            return resultUnitId;
        }

        #region Type Condition Check

        public bool ConditionSymmetry(DataCheckCondition itemCheck, List<DataCheckCondition> dataCheckConditions)
        {
            foreach (var item in dataCheckConditions)
            {
                if (item.ActivitySlot.typeSlot == itemCheck.Condition.TypeSlot)
                {
                    if (item.Index == itemCheck.Index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #endregion

        #region Function Queue Slot

        public System.Action<string, Vector3> UpdateTargetQueuePositionEvent;
        public System.Action<string> OutQueueUpdated;

        public Vector3 GetQueueCurrentPosition(string typeSlot)
        {
            var activitySlot = GetActivitySlot(typeSlot);

            if (activitySlot != null)
            {
                foreach (var item in activitySlot.queueSlots)
                {
                    if (string.IsNullOrEmpty(item.IdUnit))
                    {
                        return item.ObjSlot.transform.position;
                    }
                }
            }

            return InPosition;
        }

        public Vector3 GetQueueCurrentPosition(string typeSlot, string unitID)
        {
            var activitySlot = GetActivitySlot(typeSlot);

            if (activitySlot != null)
            {
                foreach (var item in activitySlot.queueSlots)
                {
                    if (unitID.Equals(item.IdUnit))
                    {
                        return item.ObjSlot.transform.position;
                    }
                }
            }

            return InPosition;
        }

        public bool IsInQueue(string typeSlot, string unitID)
        {
            var activitySlot = GetActivitySlot(typeSlot);

            if (activitySlot != null)
            {
                foreach (var item in activitySlot.queueSlots)
                {
                    if (unitID.Equals(item.IdUnit))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void NextQueue(ActivitySlot activitySlot)
        {
            int d = 0;
            int length = activitySlot.QueueObjs.Count;

            foreach (var item in activitySlot.queueSlots)
            {
                item.IdUnit = null;
            }

            foreach (var item in activitySlot.queueSlots)
            {
                if(d < length && activitySlot.QueueObjs[d] != null)
                {
                    item.IdUnit = activitySlot.QueueObjs[d].ID;
                }
                else
                {
                    return;
                }

                d++;
            }
        }

        #endregion

        public void IncreaseActivitySlot(ActivitySlot activity, List<BuildSlot> actionSlots, List<QueueSlot> queueSlots)
        {
            //activity.buildSlots.AddRange(actionSlots);

            if (queueSlots.Count > 0)
            {
                activity.queueSlots.AddRange(queueSlots);
                OutQueue(activity.typeSlot);
            }
        }
    }

    public class ActivityData
    {
        public string CodeName;
        public List<Transform> ActivityPoints = new List<Transform>();
        public List<Transform> QueuePoints = new List<Transform>();
        public float Duration;
    }

    [System.Serializable]
    public class SlotBase
    {
        public string IdUnit;
        public GameObject ObjSlot;
    }

    [System.Serializable]
    public class BuildSlot : SlotBase
    {
    }

    [System.Serializable]
    public class QueueSlot : SlotBase
    {
    }

    [System.Serializable]
    public class ActivitySlot
    {
        public string CodeName = "";
        public string typeSlot = null;
        public float Duration;
        public List<BuildSlot> buildSlots = new List<BuildSlot>();
        public List<UnitBehavior> QueueObjs = new List<UnitBehavior>();
        public List<QueueSlot> queueSlots = new List<QueueSlot>();

        public int GetCountSlotActive()
        {
            int d = 0;
            foreach (var item in buildSlots)
            {
                if (string.IsNullOrEmpty(item.IdUnit))
                {
                    d++;
                }
            }

            return d;
        }
    }

    public enum TypeJoinQueue
    {
        Failed,
        Queued,
        Confirmed,
    }

    [System.Serializable]
    public class DataCheckCondition
    {
        public string IdUnit;
        public ActivitySlot ActivitySlot;
        public int Index;
        public Condition Condition;

    }
}