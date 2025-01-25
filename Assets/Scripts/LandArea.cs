using Hiker.Idle;
using Hiker.Merge;
using System.Collections.Generic;

public class LandArea
{
    private BuildObject building;
    protected MergeBoard mergeBoard;
    protected MergeGround mergeGround;
    protected MergeGroundPlaceholder placeholder;
    public MergeBoard MergeBoard { get => mergeBoard; set => mergeBoard = value; }
    public BuildObject Building { get => building; set => building = value; }

    public LandArea(BuildObject building, MergeBoard board, MergeGround ground, MergeGroundPlaceholder placeholder)
    {
        this.building = building;
        mergeBoard = board;
        mergeGround = ground;
        this.placeholder = placeholder;
    }
    public void UpgradeBuilding()
    {
        var buildingGroupCodeName = building.CodeNameBuilding;
        var buildingConfig = ConfigManager.Instance.GetBuildingConfig(buildingGroupCodeName);
        if (buildingConfig == null) { return; }

        var activityPlaceholders = placeholder.GetComponentsInChildren<ActivityPlaceholder>();
        foreach (var activityPlaceholder in activityPlaceholders)
        {
            var activity = building.GetActivitySlot(activityPlaceholder.CodeName);
            if (activity == null) { continue; }

            var queueSlots = activity.queueSlots;
            int currentQueueCount = queueSlots.Count;
            int newQueueCount = currentQueueCount + buildingConfig.QueueIncreaseAmount;
            var queueNewPositions = activityPlaceholder.QueuePlaceholder.GetPositionPoints(newQueueCount);
            var addedQueueSlots = new List<QueueSlot>();
            for (int i = 0; i < queueNewPositions.Count; i++)
            {
                if (i < currentQueueCount)
                {
                    queueSlots[i].ObjSlot.transform.position = queueNewPositions[i];
                }
                else
                {
                    var queueSlot = CreateQueueSlot(activityPlaceholder.CodeName, queueNewPositions[i], building.transform);
                    addedQueueSlots.Add(queueSlot);
                }
            }

            var actionSlots = activity.buildSlots;
            int currentActionSlotCount = actionSlots.Count;
            int newActionSlotCount = currentActionSlotCount + buildingConfig.CapacityIncreaseAmount;
            var actionNewPositions = activityPlaceholder.ActionPlaceholder.GetPositionPoints(newActionSlotCount);
            var addedActionSlots = new List<BuildSlot>();
            for (int i = 0; i < actionNewPositions.Count; i++)
            {
                if (i < currentActionSlotCount)
                {
                    actionSlots[i].ObjSlot.transform.position = actionNewPositions[i];
                }
                else
                {
                    var actionSlot = CreateActionSlot(activityPlaceholder.CodeName, actionNewPositions[i], building.transform);
                    addedActionSlots.Add(actionSlot);
                }
            }

            building.IncreaseActivitySlot(activity, actionSlots, queueSlots);
        }
    }

    public static QueueSlot CreateQueueSlot(string activityCodeName, UnityEngine.Vector3 position, UnityEngine.Transform parent)
    {
        var queuePoint = new UnityEngine.GameObject($"Queue Point {activityCodeName}").transform;
        queuePoint.position = position;
        queuePoint.parent = parent;
        var queueSlot = new QueueSlot()
        {
            IdUnit = null,
            ObjSlot = queuePoint.gameObject,
        };
        return queueSlot;
    }

    public static BuildSlot CreateActionSlot(string activityCodeName, UnityEngine.Vector3 position, UnityEngine.Transform parent)
    {
        var actionPoint = new UnityEngine.GameObject($"Action Point {activityCodeName}").transform;
        actionPoint.position = position;
        actionPoint.parent = parent;
        var actionSlot = new BuildSlot()
        {
            IdUnit = null,
            ObjSlot = actionPoint.gameObject,
        };
        return actionSlot;
    }
}
