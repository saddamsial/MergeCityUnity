using Hiker;
using Hiker.Merge;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [SerializeField]
    protected MergeGround mergeGround;
    [SerializeField]
    protected CameraController cameraController;

    private void Awake()
    {
        ConfigManager.Init();
    }

    public void ShowBoard()
    {
        var mergeBoardConfig = ConfigManager.Instance.OtherConfig.TestMergeBoardConfig;
        var mergeBoard = MergeBoardConfig.PrepareMergeBoard(mergeBoardConfig);
        
        var mergeItems = new List<MergeItem>();
        var checkAddItemSpecificFuncs = new List<MergeBoard.CheckAddItemSpecific>();
        var mergeItemsConfig = mergeBoardConfig.Items;
        foreach (var mergeItemConfig in mergeItemsConfig)
        {
            var itemConfig = ConfigManager.Instance.GetItemConfig(mergeItemConfig.CodeName);
            var mergeItem = ItemConfig.CreateMergeItem(itemConfig);
            mergeItems.Add(mergeItem);

            var pivotCoord = new Coord(mergeItemConfig.RowIndex, mergeItemConfig.ColumnIndex);
            var slotCoords = MergeUtility.GetCoordsFromMatrix(pivotCoord, itemConfig.SpaceMatrix);
            checkAddItemSpecificFuncs.Add(() =>
            {
                return new MergeBoard.CheckAddItemResult()
                {
                    Success = true,
                    SlotCoords = slotCoords,
                };
            });
        }

        var connector = new MergeBoardConnector();
        connector.Init(new MergeBoardConnector.Data()
        {
            MergeBoard = mergeBoard,
            InitMergeGroundHandler = (mergeBoard) =>
            {
                InitMergeGround(mergeBoard, mergeBoardConfig);
            },
            InitMergeItems = mergeItems,
            InitCheckAddItemSpecificFuncs = checkAddItemSpecificFuncs,
        });
    }

    protected void InitMergeGround(MergeBoard mergeBoard, MergeBoardConfig mergeBoardConfig)
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
