using Hiker;
using Hiker.Merge;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MergeGround : MonoBehaviour
{
    [SerializeField]
    protected Transform groundBlocksContainer;
    [SerializeField]
    protected Transform groundCheck;
    [SerializeField]
    protected Transform itemBlocksContainer;
    [SerializeField]
    protected LockLayerContainer lockLayerContainer;
    //[SerializeField]
    protected Camera mainCamera => data.MainCamera;
    [SerializeField]
    protected Transform selectIndicator;
    [SerializeField]
    protected LayerMask itemLayerMask;
    [SerializeField]
    protected LayerMask groundLayerMask;
    [SerializeField]
    protected float dragThreshold;

    protected bool hasInit;
    protected Data data;
    protected MergeBoard mergeBoard => data.MergeBoard;
    protected float boardWidth;
    protected float boardHeight;

    protected Dictionary<string, GroundBlock> groundBlocks = new Dictionary<string, GroundBlock>();
    protected Dictionary<string, ItemBlock> itemBlocks = new Dictionary<string, ItemBlock>();

    public void Init(Data data)
    {
        this.data = data;

        boardWidth = data.MaxColumnCount * BLOCK_WIDTH + (data.MaxColumnCount - 1) * BLOCK_GAP;
        boardHeight = data.MaxRowCount * BLOCK_HEIGHT + (data.MaxRowCount - 1) * BLOCK_GAP;
        groundCheck.localScale = new Vector3(boardWidth, 1, boardHeight);

        groundBlocks = CreateGroundBlocks(mergeBoard.SlotMap.Values.ToList(), groundBlocksContainer);

        mergeBoard.OnAddItem += HandleItemAdded;
        mergeBoard.OnPutItemInBag += HandleItemPutInBag;
        mergeBoard.OnItemRemoved += HandleItemRemoved;
        mergeBoard.OnPutItemInLockLayer += HandleItemPutInLockLayer;
        mergeBoard.OnRemoveItemFromLockLayer += HandleItemRemovedFromLockLayer;
        mergeBoard.OnItemMoved += HandleItemMoved;

        hasInit = true;
    }

    protected void Update()
    {
        if (!hasInit)
        {
            return;
        }

        #region Testing
        if (Input.GetKeyUp(KeyCode.E))
        {
            mergeBoard.RemoveItem(selectedItemBlock.ID);
        }
        #endregion

        if (Input.GetMouseButtonDown(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, float.MaxValue, itemLayerMask))
            {
                var itemBlock = hit.collider.GetComponentInParent<ItemBlock>();
                if (itemBlock != null)
                {
                    HandleItemPointerDown(itemBlock);
                }
            }
        }

        if (mouseDownItemBlock != null)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, float.MaxValue, groundLayerMask))
            {
                var followPosition = new Vector3(hit.point.x, 0, hit.point.z);
                HandleItemPointerDrag(mouseDownItemBlock, followPosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (mouseDownItemBlock != null)
            {
                HandleItemPointerUp(mouseDownItemBlock, mergeOntoItemID);
            }
            else
            {
                selectIndicator.gameObject.SetActive(false);
            }

            mouseDownItemBlock = null;
            mergeOntoItemID = "";
            moveCoordsCache = null;
        }
    }

    #region Pointer Events
    protected ItemBlock mouseDownItemBlock;
    protected ItemBlock selectedItemBlock;
    protected List<Coord> moveCoordsCache;
    protected string mergeOntoItemID;
    public event System.Action OnItemPointerDown;
    protected void HandleItemPointerDown(ItemBlock itemBlock)
    {
        mouseDownItemBlock = itemBlock;
        OnItemPointerDown?.Invoke();
        //mouseDownItemConfigCache = ConfigManager.Instance.GetItemConfig(itemBlock.CodeName);
    }

    protected void HandleItemPointerDrag(ItemBlock itemBlock, Vector3 dragPosition)
    {
        if (!itemBlock.IsMovable) { return; }

        float dragSqrDist = (dragPosition - itemBlock.transform.position).sqrMagnitude;
        if (dragSqrDist > Mathf.Pow(dragThreshold, 2))
        {
            if (itemBlock != selectedItemBlock)
            {
                SelectItemBlock(itemBlock);
            }
            var hitPointOnGround = groundCheck.InverseTransformPointUnscaled(dragPosition);
            float interpolatedRowIndex = (-hitPointOnGround.z + boardHeight / 2 - HALF_BLOCK_HEIGHT) / BLOCK_HEIGHT_GAP;
            float interpolatedColumnIndex = (hitPointOnGround.x + boardWidth / 2 - HALF_BLOCK_WIDTH) / BLOCK_WIDTH_GAP;
            var moveCoords = GetMoveCoords(itemBlock.Item, interpolatedRowIndex, interpolatedColumnIndex);
            moveCoordsCache = moveCoords;
            var itemIDs = mergeBoard.GetItemIDsInArea(moveCoords
                , (id => !string.Equals(id, mouseDownItemBlock.ID))
                );
            mergeOntoItemID = "";
            foreach (var id in itemIDs)
            {
                var mergeOntoItem = mergeBoard.GetMergeItem(id);
                if (mergeOntoItem != null
                    && mergeBoard.CheckCanMerge(mergeOntoItem, mouseDownItemBlock.Item)
                    )
                {
                    mergeOntoItemID = id;
                    break;
                }
            }
            itemBlock.Follow(dragPosition);
        }
    }

    public event System.Action OnPointerUp;
    protected void HandleItemPointerUp(ItemBlock itemBlock, string mergeOntoItemID)
    {
        if (!string.IsNullOrEmpty(mergeOntoItemID))
        {
            var mergedItemIDs = new List<string>() { itemBlock.ID };
            if (mergeBoard.MergeItems(
                mergeOntoItemID
                , mergedItemIDs
                , out int newTier
                ))
            {
                MergeItems(
                    mergeOntoItemID
                    , newTier
                    );
            }
            else
            {
                itemBlock.ResetFollow();
            }
        }
        else
        {
            if (moveCoordsCache != null)
            {
                mergeBoard.MoveBoardItem(
                    itemBlock.ID
                    , moveCoordsCache
                    );
                itemBlock.ResetFollow();
            }
            SelectItemBlock(itemBlock);
        }

        OnPointerUp?.Invoke();
    }
    #endregion

    #region MergeBoard Events
    protected void HandleItemAdded(MergeBoard.EAddItemPayload payload)
    {
        var resource = Resources.Load("Prefabs/ItemBlock");
        var itemBlock = CreateItemBlock(resource, payload.ID, payload.NewItem, itemBlocksContainer);
        itemBlocks.Add(payload.ID, itemBlock);
    }

    protected void HandleItemPutInBag(MergeBoard.EPutItemInBagPayload payload)
    {
        if (itemBlocks.TryGetValue(payload.ID, out var itemBlock))
        {
            MoveItem(itemBlock, payload.NewCoords, payload.SourceCoords);
            if (mergeBoard.CheckIfItemLock(payload.ID))
            {
                itemBlock.ToggleInteractable(false);
            }
        }
    }

    protected void HandleItemRemoved(MergeBoard.EOnRemoveItemPayload payload)
    {
        if (itemBlocks.TryGetValue(payload.ID, out var itemBlock))
        {
            RemoveItemBlock(itemBlock);
        }
    }

    protected void HandleItemPutInLockLayer(MergeBoard.EPutItemInLockLayerPayload payload)
    {
        if (itemBlocks.TryGetValue(payload.ID, out var itemBlock))
        {
            MoveItem(itemBlock, payload.SlotCoords);
            lockLayerContainer.Add(itemBlock);

            var itemIDsInArea = mergeBoard.GetItemIDsInArea(payload.SlotCoords
                , (id) => !id.Equals(payload.ID)
                );
            foreach (string lockedItemID in itemIDsInArea)
            {
                if (itemBlocks.TryGetValue(lockedItemID, out var lockedItemBlock))
                {
                    lockedItemBlock.ToggleInteractable(false);
                }
            }
        }
    }

    protected void HandleItemRemovedFromLockLayer(MergeBoard.ERemoveItemFromLockLayerPayload payload)
    {
        lockLayerContainer.Remove(payload.ID);

        var itemIDsInArea = mergeBoard.GetItemIDsInArea(payload.LastCoords
            , (id) => !id.Equals(payload.ID)
            );
        foreach (string lockedItemID in itemIDsInArea)
        {
            if (itemBlocks.TryGetValue(lockedItemID, out var lockedItemBlock))
            {
                if (!mergeBoard.CheckIfItemLock(lockedItemID))
                {
                    lockedItemBlock.ToggleInteractable(true);
                }
            }
        }
    }

    protected void HandleItemMoved(MergeBoard.EItemMovedPayload payload)
    {
        if (!itemBlocks.TryGetValue(payload.ID, out var itemBlock)) { return; }

        MoveItem(itemBlock, payload.NewSlotCoords, payload.OldSlotCoords, payload.MoveStep);
    }
    #endregion

    protected void RemoveItemBlock(ItemBlock itemBlock)
    {
        if (itemBlock != null)
        {
            Destroy(itemBlock.gameObject);
            itemBlocks.Remove(itemBlock.ID);
        }
    }

    protected void MergeItems(
        string mergeOntoItemID
        , int newTier
        )
    {
        if (itemBlocks.TryGetValue(mergeOntoItemID, out var mergeOntoItemBlock))
        {
            mergeOntoItemBlock.UpdateIcon(newTier);
            SelectItemBlock(mergeOntoItemBlock);
        }
    }

    protected List<Coord> GetMoveCoords(MergeItem item, float interpolatedRowIndex, float interpolatedColumnIndex)
    {
        int maxRowCount = item.SpaceMatrix.Count;
        int maxColumnCount = item.SpaceMatrix[0].Count;

        float middleRowIndex;
        if (maxRowCount % 2 != 0)
        {
            middleRowIndex = Mathf.FloorToInt(interpolatedRowIndex + 0.5f);
        }
        else
        {
            middleRowIndex = Mathf.FloorToInt(interpolatedRowIndex) + 0.5f;
        }
        float middleColumnIndex;
        if (maxColumnCount % 2 != 0)
        {
            middleColumnIndex = Mathf.FloorToInt(interpolatedColumnIndex + 0.5f);
        }
        else
        {
            middleColumnIndex = Mathf.FloorToInt(interpolatedColumnIndex) + 0.5f;
        }

        int targetRowIndex = (int)(middleRowIndex - maxRowCount / 2f + 0.5f);
        int targetIndex = (int)(middleColumnIndex - maxColumnCount / 2f + 0.5f);
        var coords = MathUtility.GetCoordsFromMatrix(
            new Coord(targetRowIndex, targetIndex), item.SpaceMatrix);
        return coords;
    }

    protected Dictionary<string, GroundBlock> CreateGroundBlocks(List<Slot> slots, Transform parent = null)
    {
        var blocks = new Dictionary<string, GroundBlock>();
        var resource = Resources.Load("Prefabs/GroundBlock");
        foreach (var slot in slots)
        {
            var coord = new Coord()
            {
                RowIndex = slot.RowIndex,
                ColumnIndex = slot.ColumnIndex,
            };
            var groundBlock = CreateGroundBlock(resource, parent);
            groundBlock.Init(new GroundBlock.Data()
            {
                RowIndex = slot.RowIndex,
                ColumnIndex = slot.ColumnIndex,
            });
            var position = CalculatePositionFromIndexes(coord.RowIndex, coord.ColumnIndex);
            groundBlock.UpdatePosition(position);
            blocks.Add(coord.ToString(), groundBlock);
        }

        return blocks;
    }

    protected void SelectItemBlock(ItemBlock itemBlock)
    {
        selectedItemBlock = itemBlock;

        var position = itemBlock.transform.position;
        selectIndicator.transform.position = new Vector3(position.x, 0, position.z);

        var item = itemBlock.Item;
        int maxRowCount = item.SpaceMatrix.Count;
        int maxColumnCount = item.SpaceMatrix[0].Count;

        selectIndicator.transform.localScale = new Vector3(maxColumnCount, 1, maxRowCount);
        selectIndicator.gameObject.SetActive(true);
    }

    protected void MoveItem(ItemBlock itemBlock, List<Coord> newCoords, List<Coord> oldCoords = null, int moveStep = 0)
    {
        var position = CalculatePositionFromCoordGroup(newCoords);
        Vector2? oldPosition = null;
        if (oldCoords != null && oldCoords.Count > 0)
        {
            oldPosition = CalculatePositionFromCoordGroup(oldCoords);
        }
        itemBlock.UpdatePosition(position, oldPosition, moveStep);
        var size = CalculateCardSizeFromCoords(newCoords);
        itemBlock.UpdateSize(size);
    }

    protected Vector3 CalculatePositionFromCoordGroup(List<Coord> itemSpace)
    {
        //float avgRowIndex = 0;
        //float avgColumnIndex = 0;
        //foreach (var coord in itemSpace)
        //{
        //    avgRowIndex += coord.RowIndex;
        //    avgColumnIndex += coord.ColumnIndex;
        //}
        //avgRowIndex /= itemSpace.Count;
        //avgColumnIndex /= itemSpace.Count;
        //var position = CalculatePositionFromIndexes(avgRowIndex, avgColumnIndex);
        //return position;
        return CalculatePositionFromCoordGroup(itemSpace, Offset, transform.position);
    }

    public static Vector3 CalculatePositionFromCoordGroup(List<Coord> itemSpace, Vector3 offset, Vector3 origin)
    {
        float avgRowIndex = 0;
        float avgColumnIndex = 0;
        foreach (var coord in itemSpace)
        {
            avgRowIndex += coord.RowIndex;
            avgColumnIndex += coord.ColumnIndex;
        }
        avgRowIndex /= itemSpace.Count;
        avgColumnIndex /= itemSpace.Count;
        var position = CalculatePositionFromIndexes(avgRowIndex, avgColumnIndex, offset, origin);
        return position;
    }

    protected Vector3 offset;
    public Vector3 Offset
    {
        get { return offset; }
        set
        {
            offset = new Vector3(value.x - 1 / 2f, 0, value.z - 1 / 2f);
        }
    }
    protected Vector3 CalculatePositionFromIndexes(float rowIndex, float columnIndex)
    {
        //float slotWidthOffset = BLOCK_WIDTH + BLOCK_GAP;
        //float slotHeightOffset = BLOCK_HEIGHT + BLOCK_GAP;
        //float xPos = (columnIndex - Offset.x) * slotWidthOffset + transform.position.x;
        //float zPos = -(rowIndex - Offset.z) * slotHeightOffset + transform.position.z;
        //return new Vector3(xPos, 0, zPos);
        return CalculatePositionFromIndexes(rowIndex, columnIndex, Offset, transform.position);
    }

    public static Vector3 CalculatePositionFromIndexes(float rowIndex, float columnIndex, Vector3 offset, Vector3 origin)
    {
        float slotWidthOffset = BLOCK_WIDTH + BLOCK_GAP;
        float slotHeightOffset = BLOCK_HEIGHT + BLOCK_GAP;
        float xPos = (columnIndex - offset.x) * slotWidthOffset + origin.x;
        float zPos = -(rowIndex - offset.z) * slotHeightOffset + origin.z;
        return new Vector3(xPos, 0, zPos);
    }

    protected static Vector3 CalculateCardSizeFromCoords(List<Coord> coords)
    {
        int rowRange = MathUtility.GetMaxRange(coords.Select(coordTemp => coordTemp.RowIndex).ToList()) + 1;
        int columnRange = MathUtility.GetMaxRange(coords.Select(coordTemp => coordTemp.ColumnIndex).ToList()) + 1;
        return new Vector3(BLOCK_WIDTH * columnRange, 1, BLOCK_HEIGHT * rowRange);
    }

    protected static GroundBlock CreateGroundBlock(Object resource, Transform parent = null)
    {
        var go = Instantiate(resource, parent) as GameObject;
        var groundBlock = go.GetComponent<GroundBlock>();
        return groundBlock;
    }

    protected static ItemBlock CreateItemBlock(Object resource, string id, MergeItem item, Transform parent = null)
    {
        var go = Instantiate(resource, parent) as GameObject;
        var itemBlock = go.GetComponent<ItemBlock>();
        itemBlock.Init(new ItemBlock.Data(id)
        {
            ID = id,
            Item = item,
        });
        return itemBlock;
    }

    protected static readonly float BLOCK_GAP = 0f;
    public static readonly float BLOCK_WIDTH = 1;
    public static readonly float BLOCK_WIDTH_GAP = BLOCK_WIDTH + BLOCK_GAP;
    public static readonly float HALF_BLOCK_WIDTH = BLOCK_WIDTH / 2;
    public static readonly float BLOCK_HEIGHT = 1;
    public static readonly float BLOCK_HEIGHT_GAP = BLOCK_HEIGHT + BLOCK_GAP;
    public static readonly float HALF_BLOCK_HEIGHT = BLOCK_HEIGHT / 2;

    [System.Serializable]
    public class Data
    {
        public MergeBoard MergeBoard;
        public Camera MainCamera;
        public int MaxRowCount;
        public int MaxColumnCount;
    }
}
