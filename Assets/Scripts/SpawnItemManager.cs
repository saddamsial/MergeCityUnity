using Hiker;
using Hiker.Idle;
using Hiker.Merge;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Hiker.Merge.MergeBoard;

public class SpawnItemManager : MonoBehaviour
{
    [SerializeField]
    private LandAreaManager landAreaManager;

    private float spawnInterval;
    private void Start()
    {
       // Init();
    }
    public void Init()
    {
        spawnInterval = ConfigManager.Instance.SpawnConfig.BaseSpawnInterval;
        StartCoroutine(SpawnItemRepeatedly(spawnInterval));
    }
    public IEnumerator SpawnItemRepeatedly(float spawnInterval)
    {
        int randomItemIndex;
        string ItemCodeName;
        ItemConfig itemConfig;
        List<LandArea> landAreas;
        MergeItem mergeItem;

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            randomItemIndex = Random.Range(0, ConfigManager.Instance.SpawnConfig.ItemPool.Count);

            ItemCodeName = ConfigManager.Instance.SpawnConfig.ItemPool[randomItemIndex].CodeName;
            itemConfig = ConfigManager.Instance.GetItemConfig(ItemCodeName);
           
            mergeItem = new MergeItem(itemConfig.CodeName);
            Debug.Log("Item code name " + mergeItem.CodeName);
            landAreas = landAreaManager.GetLandAreas(itemConfig.BuildingCodeName);
            Debug.Log("Item building code name " + itemConfig.BuildingCodeName);
            foreach (var land in landAreas)
            {
            
                CheckAddItemResult checkAddItemSpecific = new CheckAddItemResult();
                checkAddItemSpecific.Success = true;
                
                var slotCoords= land.MergeBoard.GetCoordGroupForItem(itemConfig.SpaceMatrix, (Slot slot) =>
                {
                    return !slot.IsOccupied;
                });
              
                land.MergeBoard.AddItem(mergeItem,checkAddItemSpecific:() =>
                {
                    return new CheckAddItemResult
                    {
                        Success = true,
                        SlotCoords = slotCoords[0],
                    };
                });
               
            }
            
            landAreas.Clear();
        }
    }
    public void SpawnMergeItem()
    {
        int randomItemIndex;
        string ItemCodeName;
        ItemConfig itemConfig;
        List<LandArea> landAreas;
        MergeItem mergeItem;

        randomItemIndex = Random.Range(0, ConfigManager.Instance.SpawnConfig.ItemPool.Count);

        ItemCodeName = ConfigManager.Instance.SpawnConfig.ItemPool[randomItemIndex].CodeName;
        itemConfig = ConfigManager.Instance.GetItemConfig(ItemCodeName);

        mergeItem = new MergeItem(itemConfig.CodeName);
        mergeItem.Init(
            maxUseCount: itemConfig.MaxUseCount,
            spaceMatrix: itemConfig.SpaceMatrix);
        Debug.Log("Item code name " + mergeItem.CodeName);
        landAreas = landAreaManager.GetLandAreas(itemConfig.BuildingCodeName);
        Debug.Log("Item building code name " + itemConfig.BuildingCodeName);
        foreach (var land in landAreas)
        {

            CheckAddItemResult checkAddItemSpecific = new CheckAddItemResult();
            checkAddItemSpecific.Success = true;

            var slotCoords = land.MergeBoard.GetCoordGroupForItem(itemConfig.SpaceMatrix, (Slot slot) =>
            {
                return !slot.IsOccupied;
            });
            if (slotCoords.Count == 0)
            {
                continue;
            }
            land.MergeBoard.AddItem(mergeItem, checkAddItemSpecific: () =>
            {
                return new CheckAddItemResult
                {
                    Success = true,
                    SlotCoords = slotCoords[0],
                };
            });

        }

        landAreas.Clear();
    }

   


}