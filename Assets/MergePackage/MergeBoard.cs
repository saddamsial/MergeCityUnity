using System.Collections.Generic;
using System.Linq;

namespace Hiker.Merge
{
    public class MergeBoard
    {
        public Dictionary<string, MergeItem> ItemMap { get; protected set; } = new Dictionary<string, MergeItem>();
        public Dictionary<string, MergeItem> LockLayerItemMap = new Dictionary<string, MergeItem>();
        public Dictionary<string, MergeItem> PocketItemMap = new Dictionary<string, MergeItem>();

        public Dictionary<string, Slot> SlotMap { get; protected set; } = new Dictionary<string, Slot>();
        public List<Slot> ColumnFirstOrderedSlots { get; protected set; } = new List<Slot>();
        public List<Slot> RowFirstOrderedSlots { get; protected set; } = new List<Slot>();

        public void Init()
        {
            foreach (var slotData in SlotMap)
            {
                slotData.Value.ParseFromCoord(Coord.GetCoordFromKey(slotData.Key));
            }
            ColumnFirstOrderedSlots = SlotMap.Values
                .OrderBy(slot => slot.RowIndex)
                .OrderBy(slot => slot.ColumnIndex)
                .ToList();
            RowFirstOrderedSlots = SlotMap.Values
                .OrderBy(slot => slot.ColumnIndex)
                .OrderBy(slot => slot.RowIndex)
                .ToList();
        }

        #region Using items
        public event System.Action<EUseItemPayload> OnUseItem;
        public event System.Func<ECheckUseItemPayload, bool> CheckUseItemEvent;
        public bool UseItem(string id, string targetItemID = "")
        {
            var item = GetMergeItem(id);
            if (item == null)
            {
                return false;
            }

            bool canUse = true;
            if (CheckUseItemEvent != null)
            {
                canUse = CheckUseItemEvent.Invoke(new ECheckUseItemPayload()
                {
                    ID = id,
                    Item = item,
                });
            }
            if (!canUse)
            {
                return false;
            }

            item.Use();
            OnUseItem?.Invoke(new EUseItemPayload()
            {
                ID = id,
                TargetItemID = targetItemID,
            });

            bool outOfStock = !item.HasUnlimitedUse && !item.HasUseCount();
            if (outOfStock)
            {
                RemoveItem(id);
            }
            return true;
        }
        #endregion

        #region Merging items
        public event System.Func<CheckItemMaxTierPayload, bool> CheckItemMaxTier;
        public bool CheckCanMerge(MergeItem mergeOntoItem, MergeItem mergedItem)
        {
            bool checkItemMaxTier = false;
            if (CheckItemMaxTier != null)
            {
                checkItemMaxTier = CheckItemMaxTier.Invoke(new CheckItemMaxTierPayload()
                {
                    MergeOntoItem = mergeOntoItem,
                });
            }
            return !checkItemMaxTier
                && string.Equals(mergeOntoItem.CodeName, mergedItem.CodeName)
                && mergeOntoItem.Tier == mergedItem.Tier;
        }

        public bool CheckCanMerge(string mergeOntoItemID, string mergedItemID)
        {
            var mergeOntoItem = GetMergeItem(mergeOntoItemID);
            var mergedItem = GetMergeItem(mergedItemID);
            if (mergeOntoItem == null || mergedItem == null)
            {
                return false;
            }

            return CheckCanMerge(mergeOntoItem, mergedItem);
        }

        public event System.Action<EMergeItemsPayload> OnMergeItems;
        public event System.Func<MergeItem, int> GetMergeItemNextTier;
        public bool MergeItems(
            string mergeOntoItemID
            , List<string> mergedItemIDs
            , out int newTier
            )
        {
            newTier = 0;
            var mergeOntoItem = GetMergeItem(mergeOntoItemID);
            if (mergeOntoItem != null)
            {
                int nextTier = mergeOntoItem.Tier + 1;
                if (GetMergeItemNextTier != null)
                {
                    nextTier = GetMergeItemNextTier.Invoke(mergeOntoItem);
                }
                newTier = nextTier;
                mergeOntoItem.SetTier(newTier);
                foreach (var mergedItemID in mergedItemIDs)
                {
                    RemoveItem(mergedItemID);
                }
                OnMergeItems?.Invoke(new EMergeItemsPayload()
                {
                    MergeOntoItemID = mergeOntoItemID,
                    MergedItemIDs = mergedItemIDs,
                });

                return true;
            }
            return false;
        }
        #endregion

        #region Adding, moving and removing items
        public event System.Func<string> GenerateID;
        public event System.Action<EItemPostAddedPayload> OnItemAdded;
        public event System.Func<CheckAddItemPayload, CheckAddItemResult> CheckAddItem;
        public event System.Func<CheckKickItemsPayload, CheckKickItemsResult> CheckKickItems;
        public delegate CheckAddItemResult CheckAddItemSpecific();
        public event System.Action<EAddItemPayload> OnAddItem;
        public event System.Func<string, bool> CheckCanLock;
        public MergeItem AddItem(MergeItem newItem, AddItemExtraData extraData = null, CheckAddItemSpecific checkAddItemSpecific = null)
        {
            string id = GenerateID.Invoke();

            bool shouldCreate = true;
            if (extraData != null)
            {
                shouldCreate = extraData.ShouldCreate;
            }
            if (!shouldCreate)
            {
                id = extraData.CreatedItemID;
            }
            CheckAddItemResult checkAddResult = null;
            if (checkAddItemSpecific != null)
            {
                if (shouldCreate)
                {
                    ItemMap.Add(id, newItem);
                }
                checkAddResult = checkAddItemSpecific();
            }
            else if (CheckAddItem != null)
            {
                ItemMap.Add(id, newItem);
                checkAddResult = CheckAddItem.Invoke(new CheckAddItemPayload()
                {
                    ID = id,
                    CodeName = newItem.CodeName,
                    Tier = newItem.Tier,
                });
            }

            if (checkAddResult != null)
            {
                bool sucess = checkAddResult.Success;
                if (sucess)
                {
                    if (shouldCreate)
                    {
                        OnAddItem?.Invoke(new EAddItemPayload()
                        {
                            ID = id,
                            NewItem = newItem,
                        });
                    }

                    CheckKickItemsResult checkKickItemsResult = null;
                    if (CheckKickItems != null)
                    {
                        checkKickItemsResult = CheckKickItems?.Invoke(new CheckKickItemsPayload()
                        {
                            ID = id,
                            Area = checkAddResult.SlotCoords,
                        });
                    }
                    if (checkKickItemsResult != null)
                    {
                        foreach (var kickedItemEntry in checkKickItemsResult.KickedItems)
                        {
                            string kickedItemID = kickedItemEntry.Key;
                            var kickedItemData = kickedItemEntry.Value;
                            if (kickedItemData.ToPocket)
                            {
                                if (CheckIfItemInLockLayer(kickedItemID))
                                {
                                    RemoveLockItem(kickedItemID);
                                }
                                else
                                {
                                    RemoveBoardItem(kickedItemID);
                                }
                                PutItemInPocket(kickedItemID);
                            }
                            else
                            {
                                MoveItem(kickedItemID, kickedItemData.Coords);
                            }
                        }
                    }
                    if (extraData != null)
                    {
                        checkAddResult.SourceCoords = extraData.SourceCoord;
                    }
                    ProcessAddItemResult(id, checkAddResult);

                    OnItemAdded?.Invoke(new EItemPostAddedPayload()
                    {
                        ID = id,
                    });
                    return newItem;
                }
                else
                {
                    ItemMap.Remove(id);
                }
            }

            return null;
        }

        public event System.Action<EItemsPostBatchAddedPayload> OnItemsPostBatchAdded;
        public void AddItemsBatch(List<MergeItem> newItems, List<CheckAddItemSpecific> checkAddItemSpecifics)
        {
            int count = newItems.Count;
            var ids = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var item = newItems[i];
                var checkAddResult = checkAddItemSpecifics[i]();

                string id = GenerateID.Invoke();
                ids.Add(id);

                ItemMap.Add(id, item);
                OnAddItem?.Invoke(new EAddItemPayload()
                {
                    ID = id,
                    NewItem = item,
                });
                ProcessAddItemResult(id, checkAddResult);
            }

            OnItemsPostBatchAdded?.Invoke(new EItemsPostBatchAddedPayload()
            {
                IDs = ids,
            });
        }

        protected void ProcessAddItemResult(string itemID, CheckAddItemResult checkAddResult)
        {
            if (CheckCanLock.Invoke(itemID))
            {
                PutItemInLockLayer(itemID, checkAddResult.SlotCoords);
            }
            else if (checkAddResult.ToBag)
            {
                PutItemInBag(itemID, checkAddResult.SlotCoords, checkAddResult.SourceCoords);
            }
            else if (checkAddResult.ToPocket)
            {
                PutItemInPocket(itemID);
            }
        }

        public event System.Action<EPutItemInLockLayerPayload> OnPutItemInLockLayer;
        public void PutItemInLockLayer(string id, List<Coord> slotCoords)
        {
            var item = GetMergeItem(id);
            if (item != null)
            {
                LockLayerItemMap.Add(id, item);
                foreach (var coord in slotCoords)
                {
                    if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                    {
                        slot.Lock(id);
                    }
                }

                OnPutItemInLockLayer?.Invoke(new EPutItemInLockLayerPayload()
                {
                    ID = id,
                    SlotCoords = slotCoords,
                });
            }
        }

        public event System.Action<EPutItemInBagPayload> OnPutItemInBag;
        public void PutItemInBag(string id, List<Coord> newCoords, List<Coord> sourceCoords = null)
        {
            sourceCoords ??= new List<Coord>();

            FillSlots(newCoords, id);
            OnPutItemInBag?.Invoke(new EPutItemInBagPayload()
            {
                ID = id,
                NewCoords = newCoords,
                SourceCoords = sourceCoords,
            });
        }

        public event System.Action<EPutItemInPocketPayload> OnPutItemInPocket;
        public void PutItemInPocket(string id)
        {
            var item = GetMergeItem(id);
            if (item != null)
            {
                PocketItemMap.Add(id, item);

                OnPutItemInPocket?.Invoke(new EPutItemInPocketPayload()
                {
                    ID = id,
                });
            }
        }

        public event System.Func<CheckCanMoveToPocketPayload, bool> CheckCanMoveToPocket;
        public event System.Action<EItemMovedPayload> OnItemMoved;
        public bool MoveBoardItem(string id, List<Coord> coords)
        {
            if (ItemMap.TryGetValue(id, out var inventoryItem))
            {
                if (CheckIfItemInLockLayer(id))
                {
                    return false;
                }

                var newCoords = coords;
                if (!CheckCoordsInValidRange(newCoords))
                {
                    return false;
                }

                List<Coord> oldCoords = GetItemBoardCoords(id);
                bool isItemInPocket = CheckIfItemInPocket(id);
                if (!MathUtility.GetCoordDiff(newCoords, oldCoords, out var coordDiff))
                {
                    if (!isItemInPocket)
                    {
                        return false;
                    }
                }

                var moveToAreaItemIDs = GetItemIDsInArea(newCoords
                    , (eachItemID => !string.Equals(eachItemID, id)));
                var getSwappedItemIDs = new List<string>();
                var toPocketItemIDs = new List<string>();
                if (CheckCanMoveToPocket == null) { CheckCanMoveToPocket += (_ => true); }
                if (CheckCanHover == null) { CheckCanHover = (_ => false); }
                foreach (var itemID in moveToAreaItemIDs)
                {
                    var item = GetMergeItem(itemID);
                    if (item == null) { return false; }

                    bool isInLockLayer = CheckIfItemInLockLayer(itemID);
                    if (isInLockLayer)
                    {
                        if (CheckCanHover(itemID))
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    var itemSlotCoords = GetItemBoardCoords(itemID);
                    if (!isItemInPocket && MathUtility.CheckIfCoordContains(newCoords, itemSlotCoords))
                    // Item in move to area can be swapped
                    {
                        getSwappedItemIDs.Add(itemID);
                    }
                    else
                    {
                        if (!CheckCanMoveToPocket(new CheckCanMoveToPocketPayload()
                        {
                            ID = itemID
                        }))
                        {
                            return false;
                        }
                        toPocketItemIDs.Add(itemID);
                    }
                }

                // Process item to be put in pocket
                if (isItemInPocket)
                {
                    RemovePocketItem(id);
                }
                foreach (var toPocketItemID in toPocketItemIDs)
                {
                    var itemCoords = GetItemBoardCoords(toPocketItemID);
                    EmptySlots(itemCoords);
                    PutItemInPocket(toPocketItemID);
                }

                // Process move item and swapped items
                var getSwappedItemsSlotCoordCacheGroup = new List<List<Coord>>();
                for (int i = 0; i < getSwappedItemIDs.Count; i++)
                {
                    var itemSlotCoords = GetItemBoardCoords(getSwappedItemIDs[i]);
                    getSwappedItemsSlotCoordCacheGroup.Add(itemSlotCoords);
                }
                MoveItem(id, newCoords);
                var emptySlotCoords = new List<Coord>();
                for (int i = 0; i < getSwappedItemIDs.Count; i++)
                {
                    var itemSlotCoords = getSwappedItemsSlotCoordCacheGroup[i];
                    var swappedCoords = Coord.AddCoords(itemSlotCoords, coordDiff);
                    bool toPocketInstead = false;
                    foreach (var coord in swappedCoords)
                    {
                        if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                        {
                            if (!CheckIfSlotAvailable(slot))
                            {
                                toPocketInstead = true;
                                break;
                            }
                        }
                    }
                    if (toPocketInstead)
                    {
                        PutItemInPocket(getSwappedItemIDs[i]);
                    }
                    else
                    {
                        MoveItem(getSwappedItemIDs[i], swappedCoords, shouldClearSpace: false);
                    }
                }
            }
            return true;
        }

        public void MoveItem(string id, List<Coord> newSlotCoords, bool shouldClearSpace = true, int moveStep = 0)
        {
            List<Coord> oldSlotCoords;
            bool isInLockLayer = CheckIfItemInLockLayer(id);
            if (isInLockLayer)
            {
                oldSlotCoords = GetItemLockCoords(id);
                if (shouldClearSpace)
                {
                    UnlockSlots(oldSlotCoords);
                }
                LockSlots(newSlotCoords, id);
            }
            else
            {
                oldSlotCoords = GetItemBoardCoords(id);
                if (shouldClearSpace)
                {
                    EmptySlots(oldSlotCoords);
                }
                FillSlots(newSlotCoords, id);
            }

            OnItemMoved?.Invoke(new EItemMovedPayload()
            {
                ID = id,
                IsInLockLayer = isInLockLayer,
                OldSlotCoords = oldSlotCoords,
                NewSlotCoords = newSlotCoords,
                ShouldClearSpace = shouldClearSpace,
                MoveStep = moveStep,
            });
        }

        public event System.Action<EOnItemPreRemovedPayload> OnItemPreRemove;
        public event System.Action<EOnRemoveItemPayload> OnItemRemoved;
        public void RemoveItem(string ID, string removerID = "")
        {
            var item = GetMergeItem(ID);
            if (item == null) { return; }

            OnItemPreRemove?.Invoke(new EOnItemPreRemovedPayload()
            {
                ID = ID,
                RemoverID = removerID,
            });

            ItemMap.Remove(ID);
            if (CheckIfItemInLockLayer(ID))
            {
                RemoveLockItem(ID);
            }
            else if (CheckIfItemInPocket(ID))
            {
                RemovePocketItem(ID);
            }
            else
            {
                RemoveBoardItem(ID);
            }

            OnItemRemoved?.Invoke(new EOnRemoveItemPayload()
            {
                CodeName = item.CodeName,
                ID = ID,
                RemoverID = removerID,
            });
        }
        #endregion

        #region Utility
        public bool CheckIfItemLock(string id)
        {
            if (ItemMap.TryGetValue(id, out var item))
            {
                var slotCoords = GetItemBoardCoords(id);
                foreach (var coord in slotCoords)
                {
                    var slot = SlotMap[coord.ToString()];
                    if (slot.IsLock) { return true; }
                }
            }
            return false;
        }

        public delegate bool GetItemIDsInAreaCondition(string id);
        public List<string> GetItemIDsInArea(List<Coord> coords, GetItemIDsInAreaCondition condition = null)
        {
            condition = condition ?? (_ => true);
            HashSet<string> itemIDs = new HashSet<string>();
            foreach (var coord in coords)
            {
                if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                {
                    if (slot.IsLock && condition(slot.LockOccupierID))
                    {
                        itemIDs.Add(slot.LockOccupierID);
                    }
                    if (slot.IsOccupied && condition(slot.OccupierID))
                    {
                        itemIDs.Add(slot.OccupierID);
                    }
                }
            }
            return itemIDs.ToList();
        }

        public event System.Func<string, List<List<int>>> GetItemSpaceMatrix;
        public List<Coord> GetOpenSlotCoords(string itemID)
        {
            var slotCoordsGroup = GetCoordGroupForItem(
                GetItemSpaceMatrix.Invoke(itemID)
                , (slot) =>
                {
                    return CheckIfSlotAvailable(slot);
                });
            if (slotCoordsGroup.Count > 0)
            {
                return slotCoordsGroup[0];
            }

            return null;
        }

        public delegate bool GetCoordGroupForItemCondition(Slot slot);
        public List<List<Coord>> GetCoordGroupForItem(List<List<int>> spaceMatrix, GetCoordGroupForItemCondition condition = null)
        {
            condition = condition ?? (_ => true);
            var slotCoordsGroup = new List<List<Coord>>();
            foreach (var slot in RowFirstOrderedSlots)
            {
                var slotCoords = MathUtility.GetCoordsFromMatrix(new Coord(slot.RowIndex, slot.ColumnIndex), spaceMatrix);
                if (!CheckCoordsInValidRange(slotCoords)) { continue; }

                var eachCoords = new List<Coord>();
                foreach (var coord in slotCoords)
                {
                    if (!condition(SlotMap[coord.ToString()]))
                    {
                        break;
                    }
                    eachCoords.Add(coord);
                }

                if (eachCoords.Count == slotCoords.Count)
                {
                    slotCoordsGroup.Add(eachCoords);
                }
            }
            return slotCoordsGroup;
        }

        public bool CheckIfBoardFull()
        {
            foreach (var slot in SlotMap.Values)
            {
                if (CheckIfSlotAvailable(slot)) { return false; }
            }

            return true;
        }

        public event System.Func<string, bool> CheckCanHover;
        public bool CheckIfSlotAvailable(Slot slot)
        {
            if (slot.IsOccupied)
            {
                return false;
            }

            if (slot.IsLock)
            {
                if (CheckCanHover == null)
                {
                    CheckCanHover = (_ => false);
                }

                return CheckCanHover(slot.LockOccupierID);
            }

            return true;
        }

        protected void EmptySlots(List<Coord> coords)
        {
            foreach (var coord in coords)
            {
                if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                {
                    slot.Empty();
                }
            }
        }

        protected void FillSlots(List<Coord> coords, string id)
        {
            foreach (var coord in coords)
            {
                if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                {
                    slot.Fill(id);
                }
            }
        }

        protected void UnlockSlots(List<Coord> coords)
        {
            foreach (var coord in coords)
            {
                if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                {
                    slot.Unlock();
                }
            }
        }

        protected void LockSlots(List<Coord> coords, string id)
        {
            foreach (var coord in coords)
            {
                if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                {
                    slot.Lock(id);
                }
            }
        }

        public MergeItem GetMergeItem(string id)
        {
            if (ItemMap.TryGetValue(id, out var item))
            {
                return item;
            }

            return item;
        }

        public event System.Action<ERemoveItemFromLockLayerPayload> OnRemoveItemFromLockLayer;
        protected void RemoveLockItem(string id)
        {
            var lockCoords = GetItemLockCoords(id);
            foreach (var coord in lockCoords)
            {
                if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                {
                    slot.Unlock();
                }
            }
            LockLayerItemMap.Remove(id);
            OnRemoveItemFromLockLayer?.Invoke(new ERemoveItemFromLockLayerPayload()
            {
                ID = id,
                LastCoords = lockCoords,
            });
        }

        public event System.Action<ERemoveItemFromPocketPayload> OnRemoveItemFromPocket;
        protected void RemovePocketItem(string id)
        {
            PocketItemMap.Remove(id);
            OnRemoveItemFromPocket?.Invoke(new ERemoveItemFromPocketPayload()
            {
                ID = id,
            });
        }

        protected void RemoveBoardItem(string id)
        {
            var boardCoords = GetItemBoardCoords(id);
            foreach (var coord in boardCoords)
            {
                if (SlotMap.TryGetValue(coord.ToString(), out var slot))
                {
                    slot.Empty();
                }
            }
        }

        public List<Coord> GetItemCoords(string id)
        {
            if (CheckIfItemInLockLayer(id))
            {
                return GetItemLockCoords(id);
            }

            return GetItemBoardCoords(id);
        }

        protected List<Coord> GetItemLockCoords(string id)
        {
            var itemCoords = SlotMap
                .Where(slotData => string.Equals(slotData.Value.LockOccupierID, id))
                .Select(coordData => Coord.GetCoordFromKey(coordData.Key)).ToList();
            return itemCoords;
        }

        protected List<Coord> GetItemBoardCoords(string id)
        {
            var itemCoords = SlotMap
                .Where(slotData => string.Equals(slotData.Value.OccupierID, id))
                .Select(coordData => Coord.GetCoordFromKey(coordData.Key)).ToList();
            return itemCoords;
        }

        public bool CheckIfItemInPocket(string id)
        {
            return PocketItemMap.ContainsKey(id);
        }

        public bool CheckIfItemInLockLayer(string id)
        {
            return LockLayerItemMap.ContainsKey(id);
        }

        public bool CheckCoordsInValidRange(List<Coord> coords)
        {
            foreach (var coord in coords)
            {
                if (!SlotMap.ContainsKey(coord.ToString())) { return false; }
            }
            return true;
        }
        #endregion

        public class EUseItemPayload
        {
            public string ID;
            public string TargetItemID;
        }

        public class ECheckUseItemPayload
        {
            public string ID;
            public MergeItem Item;
        }

        public class EOnItemPreRemovedPayload
        {
            public string ID;
            public string RemoverID;
        }

        public class EOnRemoveItemPayload
        {
            public string CodeName;
            public string ID;
            public string RemoverID;
        }

        public class ERemoveItemFromLockLayerPayload
        {
            public string ID;
            public List<Coord> LastCoords = new List<Coord>();
        }

        public class ERemoveItemFromPocketPayload
        {
            public string ID;
        }

        public class CheckItemMaxTierPayload
        {
            public MergeItem MergeOntoItem;
        }

        public class EMergeItemsPayload
        {
            public string MergeOntoItemID;
            public List<string> MergedItemIDs;
        }

        public class AddItemExtraData
        {
            public List<Coord> SourceCoord = new List<Coord>();
            public string CreatedItemID;

            public bool ShouldCreate => string.IsNullOrEmpty(CreatedItemID);
        }

        public class CheckAddItemResult
        {
            public bool Success;
            public List<Coord> SlotCoords = new List<Coord>();
            public Dictionary<string, KickedItemData> KickedItems = new Dictionary<string, KickedItemData>();
            public List<Coord> SourceCoords = new List<Coord>();

            public bool ToBag => SlotCoords.Count > 0;
            public bool ToPocket => SlotCoords.Count == 0;
        }

        public class KickedItemData
        {
            public List<Coord> Coords = new List<Coord>();
            public bool ToPocket => Coords.Count == 0;
        }

        public class CheckAddItemPayload
        {
            public string ID;
            public string CodeName;
            public int Tier;
            public List<Coord> NotAllowedCoords = new List<Coord>();
        }

        public class EAddItemPayload
        {
            public string ID;
            public MergeItem NewItem;
            public List<Coord> slotCoords = new List<Coord>();
        }

        public class CheckKickItemsPayload
        {
            public string ID;
            public List<Coord> Area = new List<Coord>();
        }

        public class CheckKickItemsResult
        {
            public Dictionary<string, KickedItemData> KickedItems = new Dictionary<string, KickedItemData>();
        }

        public class EItemMovedPayload
        {
            public string ID;
            public bool IsInLockLayer;
            public List<Coord> OldSlotCoords = new List<Coord>();
            public List<Coord> NewSlotCoords = new List<Coord>();
            public bool ShouldClearSpace = true;
            public int MoveStep;
        }

        public class EPutItemInLockLayerPayload
        {
            public string ID;
            public List<Coord> SlotCoords = new List<Coord>();
        }

        public class EPutItemInBagPayload
        {
            public string ID;
            public List<Coord> NewCoords = new List<Coord>();
            public List<Coord> SourceCoords = new List<Coord>();
        }

        public class EPutItemInPocketPayload
        {
            public string ID;
        }

        public class EItemPostAddedPayload
        {
            public string ID;
        }

        public class EItemsPostBatchAddedPayload
        {
            public List<string> IDs;
        }

        public class CheckCanMoveToPocketPayload
        {
            public string ID;
        }
    }
}
