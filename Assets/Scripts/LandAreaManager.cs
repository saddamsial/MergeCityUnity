using Hiker;
using Hiker.Idle;
using Hiker.Merge;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LandAreaManager : MonoBehaviour
{
    [SerializeField]
    protected CameraController cameraController;

    protected Dictionary<string, BuildingBase> buildingGroups = new Dictionary<string, BuildingBase>();
    protected List<LandArea> landAreas = new List<LandArea>();

    public List<LandArea> LandAreas { get => landAreas; set => landAreas = value; }

    public void Init()
    {
        var placeholders = GetComponentsInChildren<MergeGroundPlaceholder>();
        var mergeGroundResource = Resources.Load("Prefabs/MergeGround");

        foreach (var mergeGroundPlaceholder in placeholders)
        {
            var mergeBoardConfig = new MergeBoardConfig(
                mergeGroundPlaceholder.MaxRowCount,
                mergeGroundPlaceholder.MaxColumnCount,
                mergeGroundPlaceholder.BuildingSpace
                .Select(hole => new Coord(hole.RowIndex, hole.ColumnIndex))
                .ToList());

            var mergeBoard = MergeBoardConfig.PrepareMergeBoard(mergeBoardConfig);
            var mergeGroundGo = Instantiate(mergeGroundResource, transform) as GameObject;
            mergeGroundGo.transform.position = mergeGroundPlaceholder.transform.position;
            var mergeGround = mergeGroundGo.GetComponent<MergeGround>();

            var connector = new MergeBoardConnector();
            connector.Init(new MergeBoardConnector.Data()
            {
                MergeBoard = mergeBoard,
                InitMergeGroundHandler = (mergeBoard) =>
                {
                    InitMergeGround(mergeGround, mergeBoard, mergeBoardConfig, mergeGroundPlaceholder.RotateAngle);
                },
            });

            #region Create Buildings
            var building = CreateBuilding(mergeGroundPlaceholder);
            if (building != null)
            {
                string buildingGroupCodeName = mergeGroundPlaceholder.MergeBoardCodeName;
                BuildingBase buildingGroup = GetBuildingGroup(buildingGroupCodeName);
                if (buildingGroup == null)
                {
                    buildingGroup = new BuildingBase();
                    buildingGroup.InitBuildingBase(buildingGroupCodeName);
                    buildingGroups.Add(buildingGroupCodeName, buildingGroup);
                }

                buildingGroup.InitEventBuilding(building);
                buildingGroup.AddBuildObject(building);
            }
            #endregion

            mergeGroundPlaceholder.gameObject.SetActive(false);

            var landArea = new LandArea(building, mergeBoard, mergeGround, mergeGroundPlaceholder);
            landAreas.Add(landArea);
            
        }
    }

    private void InitMergeGround(MergeGround mergeGround, MergeBoard mergeBoard, MergeBoardConfig mergeBoardConfig, float rotateAngle)
    {
        var offset = new Vector3(mergeBoardConfig.ColumnCount / 2f, 0, mergeBoardConfig.RowCount / 2f);
        mergeGround.Offset = offset;
        mergeGround.Init(new MergeGround.Data()
        {
            MergeBoard = mergeBoard,
            MaxRowCount = mergeBoardConfig.RowCount,
            MaxColumnCount = mergeBoardConfig.ColumnCount,
            MainCamera = cameraController.TargetCamera,
        });
        mergeGround.OnItemPointerDown += HandleItemPointerDown;
        mergeGround.OnPointerUp += HandleMergeGroundPointerUp;

        mergeGround.transform.rotation = Quaternion.Euler(0, rotateAngle, 0);
    }

    protected BuildObject CreateBuilding(MergeGroundPlaceholder mergeGroundPlaceholder)
    {
        var buildingConfig = ConfigManager.Instance.GetBuildingConfig(mergeGroundPlaceholder.MergeBoardCodeName);
        if (buildingConfig == null)
        {
            return null;
        }

        var buildingResource = Resources.Load($"Prefabs/Building{mergeGroundPlaceholder.MergeBoardCodeName}");
        var buildingGo = Instantiate(buildingResource) as GameObject;
        var building = buildingGo.GetComponent<BuildObject>();
        building.transform.position = mergeGroundPlaceholder.BuildingPlaceholder.transform.position;
        building.transform.rotation = mergeGroundPlaceholder.BuildingPlaceholder.transform.rotation;

        var activityDataList = new List<ActivityData>();
        var activityPlaceholders = mergeGroundPlaceholder.GetComponentsInChildren<ActivityPlaceholder>();
        foreach (var activityPlaceholder in activityPlaceholders)
        {
            var activityConfig = buildingConfig.GetActivityConfig(activityPlaceholder.CodeName);
            if (activityConfig == null) { continue; }

            activityDataList.Add(new ActivityData()
            {
                CodeName = activityPlaceholder.CodeName,
                QueuePoints = activityPlaceholder.QueuePlaceholder.GetPositionPoints(activityPlaceholder.QueuePlaceholder.PositionMaxCount).Select(
                    item =>
                    {
                        var queuePoint = new GameObject($"Queue Point {activityPlaceholder.CodeName}").transform;
                        queuePoint.position = item;
                        queuePoint.parent = building.transform;
                        return queuePoint;
                    }).ToList(),
                ActivityPoints = activityPlaceholder.ActionPlaceholder.GetPositionPoints(activityPlaceholder.ActionPlaceholder.PositionMaxCount).Select(
                    item =>
                    {
                        var actionPoint = new GameObject($"Action Point {activityPlaceholder.CodeName}").transform;
                        actionPoint.position = item;
                        actionPoint.parent = building.transform;
                        return actionPoint;
                    }).ToList(),
                Duration = activityConfig.Duration
            });
        }
        building.Init(activityDataList, mergeGroundPlaceholder.InPoint.position, mergeGroundPlaceholder.OutPoint.position);
        return building;
    }

    public BuildingBase GetBuildingGroup(string codeName)
    {
        if (buildingGroups.TryGetValue(codeName, out var buildingGroup))
        {
            return buildingGroup;
        }

        return null;
    }

    public List<LandArea> GetLandAreas(string codeName)
    {
        return landAreas.FindAll(x => x.Building.CodeNameBuilding == codeName); 
    }
    public BuildObject PrebookBuilding(string buildingGroupCodeName, UnitBehavior unitBehavior, string typeSlot = null)
    {
        BuildingBase buildingGroup = GetBuildingGroup(buildingGroupCodeName);

        if (buildingGroup == null)
        {
            return null;
        }

        BuildObject building;
        if (!string.IsNullOrEmpty(typeSlot))
        {
            building = buildingGroup.GetBuildingSlot(unitBehavior, typeSlot);
        }
        else
        {
            building = buildingGroup.GetBuildingSlot(unitBehavior);
        }

        if (building == null)
        {
            return null;
        }

        return building;
    }

    private void HandleItemPointerDown()
    {
        cameraController.ToggleCameraMovement(false);
    }

    private void HandleMergeGroundPointerUp()
    {
        cameraController.ToggleCameraMovement(true);
    }
}
