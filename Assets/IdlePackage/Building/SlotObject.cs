using UnityEngine;

namespace Hiker.Idle
{
    public class SlotObject : MonoBehaviour
    {
        [SerializeField] string typeSlot;
        public string GetTypeSlot()
        {
            return string.IsNullOrEmpty(typeSlot) ? null : typeSlot;
        }

        public void ResetSlotIsNULL()
        {
            typeSlot = null;
        }
    }
}
