using System.Collections.Generic;
using UnityEngine;

public class ItemModel : MonoBehaviour
{
    [SerializeField]
    protected List<ItemModelRenderer> models;
    protected ItemModelRenderer activeModel;

    public void SetTierModel(int tier)
    {
        if (models.Count == 0) { return; }

        foreach (var model in models)
        {
            model.gameObject.SetActive(false);
        }

        int count = models.Count;
        int validTier = Mathf.Clamp(tier, 0, count - 1);
        var activeModel = models[validTier];
        activeModel.gameObject.SetActive(true);
        this.activeModel = activeModel;
    }
}
