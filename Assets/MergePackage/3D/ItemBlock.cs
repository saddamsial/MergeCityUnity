using Hiker;
using Hiker.Merge;
using UnityEngine;

public class ItemBlock : MonoBehaviour
{
    [SerializeField]
    protected Transform modelContainer;
    [SerializeField]
    protected Collider interactCollider;

    protected Data data;
    public string ID => data.ID;
    public string CodeName => data.Item.CodeName;
    public MergeItem Item => data.Item;
    protected ItemModel model;
    public bool IsMovable { get; protected set; } = true;
    public void ToggleMovable(bool isMovable)
    {
        IsMovable = isMovable;
    }

    public void ToggleInteractable(bool isInteractable)
    {
        interactCollider.gameObject.SetActive(isInteractable);
    }

    public void Init(Data data)
    {
        this.data = data;

        model = CreateItemModel(data.Item.CodeName, modelContainer);
        UpdateIcon(data.Item.Tier);

        Item.OnActivated += HandleActivated;
    }

    public void UpdatePosition(Vector3 position, Vector2? oldPosition = null, int moveStep = 0)
    {
        transform.position = position;
    }

    public void UpdateSize(Vector3 size)
    {
        transform.localScale = size;
    }

    public void Follow(Vector3 position)
    {
        modelContainer.transform.position = position;
    }

    public void ResetFollow()
    {
        modelContainer.transform.localPosition = Vector3.zero;
    }

    #region Item Events
    protected void HandleActivated(bool activated)
    {
        //var itemConfig = ConfigManager.Instance.GetItemConfig(CodeName);
        //if (CommonUtility.CheckIsObstacle(itemConfig))
        //{
        //    model.gameObject.SetActive(true);
        //}
        //else
        //{
        //    model.gameObject.SetActive(activated);
        //}
        //model.ToggleLight(activated);
    }
    #endregion

    public class Data
    {
        public Data(string id)
        {
            ID = id;
        }

        public string ID;
        public MergeItem Item;
        public bool IsLock = false;
    }

    protected static ItemModel CreateItemModel(string codeName, Transform parent = null)
    {
        var resource = Resources.Load("Prefabs/Items/" + codeName);
        if (resource != null)
        {
            var go = Instantiate(resource, parent) as GameObject;
            var itemModel = go.GetComponent<ItemModel>();
            return itemModel;
        }

        return null;
    }

    public void UpdateIcon(int newTier)
    {
        model.SetTierModel(newTier);
    }
}
