using Hiker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Merge
{
    public class MergeBoardConnector
    {
        protected Data data;
        protected MergeBoard mergeBoard => data.MergeBoard;

        public void Init(Data data)
        {
            this.data = data;

            InitInventoryEvents(mergeBoard);

            data.InitMergeGroundHandler?.Invoke(mergeBoard);

            mergeBoard.AddItemsBatch(data.InitMergeItems, data.InitCheckAddItemSpecificFuncs);
        }

        protected void InitInventoryEvents(MergeBoard mergeBoard)
        {
            mergeBoard.GenerateID += GenerateID;
            mergeBoard.CheckUseItemEvent += HandleCheckUseItemEvent;
            mergeBoard.CheckAddItem += CheckAddItem;
            mergeBoard.CheckKickItems += CheckKickItems;
            mergeBoard.CheckCanLock += CheckCanLock;
            mergeBoard.OnUseItem += HandleUseItem;
            //mergeBoard.OnItemSold += HandleItemSold;
            mergeBoard.CheckCanMoveToPocket += CheckCanMoveToPocket;
            mergeBoard.CheckCanHover += CheckCanHover;
            mergeBoard.CheckItemMaxTier += CheckItemMaxTier;
            mergeBoard.OnItemAdded += HandleItemAdded;
            mergeBoard.OnItemsPostBatchAdded += HandleItemsPostBatchAdded;
            mergeBoard.OnMergeItems += HandleItemsMerged;
            mergeBoard.OnItemPreRemove += HandleItemPreRemove;
            mergeBoard.OnItemRemoved += HandleItemRemoved;
            mergeBoard.GetItemSpaceMatrix += GetItemSpaceMatrix;
        }

        protected string GenerateID()
        {
            return MergeUtility.GetUniqueID();
        }

        protected bool HandleCheckUseItemEvent(MergeBoard.ECheckUseItemPayload payload)
        {
            throw new NotImplementedException();
        }

        protected MergeBoard.CheckAddItemResult CheckAddItem(MergeBoard.CheckAddItemPayload payload)
        {
            var result = new MergeBoard.CheckAddItemResult()
            {
                Success = true,
            };
            var slotCoords = mergeBoard.GetOpenSlotCoords(payload.ID);
            if (slotCoords == null)
            {
                result.Success = false;
            }
            else
            {
                result.SlotCoords = slotCoords;
            }
            return result;
        }

        protected MergeBoard.CheckKickItemsResult CheckKickItems(MergeBoard.CheckKickItemsPayload payload)
        {
            return new MergeBoard.CheckKickItemsResult()
            {
                KickedItems = new Dictionary<string, MergeBoard.KickedItemData>(),
            };
        }

        protected bool CheckCanLock(string itemID)
        {
            return false;
        }

        protected void HandleUseItem(MergeBoard.EUseItemPayload payload)
        {
            throw new NotImplementedException();
        }

        protected bool CheckCanMoveToPocket(MergeBoard.CheckCanMoveToPocketPayload payload)
        {
            return true;
        }

        private bool CheckCanHover(string itemID)
        {
            return false;
        }

        protected bool CheckItemMaxTier(MergeBoard.CheckItemMaxTierPayload payload)
        {
            return false;
        }

        protected void HandleItemAdded(MergeBoard.EItemPostAddedPayload payload)
        {
            
        }

        protected void HandleItemsPostBatchAdded(MergeBoard.EItemsPostBatchAddedPayload payload)
        {
        }

        protected void HandleItemsMerged(MergeBoard.EMergeItemsPayload payload)
        {
        }

        protected void HandleItemPreRemove(MergeBoard.EOnItemPreRemovedPayload payload)
        {
        }

        protected void HandleItemRemoved(MergeBoard.EOnRemoveItemPayload payload)
        {
        }

        protected List<List<int>> GetItemSpaceMatrix(string itemID)
        {
            return new List<List<int>>() { new List<int>() { 1 } };
        }

        public delegate void InitMergeGroundHandler(MergeBoard mergeBoard);
        public delegate void InitMergeBoardHandler();
        public class Data
        {
            public MergeBoard MergeBoard;
            public InitMergeGroundHandler InitMergeGroundHandler;
            public List<MergeItem> InitMergeItems = new List<MergeItem>();
            public List<MergeBoard.CheckAddItemSpecific> InitCheckAddItemSpecificFuncs = new List<MergeBoard.CheckAddItemSpecific>();
        }
    }
}
