using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMergeItemView : MonoBehaviour
{
    [SerializeField]
    private SpawnItemManager spawnItemManager;
    [SerializeField]
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        if (gameManager.energyController.CurrentEnergy <= 0) return;
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Kiểm tra xem đối tượng chúng ta chạm có phải là đối tượng cần thực hiện hành động hay không
            if (hit.transform.gameObject == this.gameObject)
            {
                // Thực hiện hành động của bạn ở đây
                spawnItemManager.SpawnMergeItem();
                gameManager.energyController.UpdateEnergy(-1);
            }
        }
    }
}
