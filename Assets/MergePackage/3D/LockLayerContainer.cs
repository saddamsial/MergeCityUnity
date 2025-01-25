using System.Collections.Generic;
using UnityEngine;

public class LockLayerContainer : MonoBehaviour
{
    protected Dictionary<string, ItemBlock> lockItemMap = new Dictionary<string, ItemBlock>();

    public void Add(ItemBlock itemBlock)
    {
        if (lockItemMap.TryAdd(itemBlock.ID, itemBlock))
        {
            itemBlock.ToggleMovable(false);
            itemBlock.transform.SetParent(transform);
        }

    }

    public void Remove(string id)
    {
        lockItemMap.Remove(id);
    }
}
