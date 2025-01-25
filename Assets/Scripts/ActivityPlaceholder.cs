using UnityEngine;

public class ActivityPlaceholder : MonoBehaviour
{
    [SerializeField]
    protected string codeName;
    public string CodeName => codeName;
    [SerializeField]
    protected QueuePlaceholder queuePlaceholder;
    public QueuePlaceholder QueuePlaceholder => queuePlaceholder;
    [SerializeField]
    protected ActionPlaceholder actionPlaceholder;
    public ActionPlaceholder ActionPlaceholder => actionPlaceholder;
}
