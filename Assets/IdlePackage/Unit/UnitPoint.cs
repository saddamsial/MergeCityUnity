using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public class UnitPoint : MonoBehaviour
    {
        [SerializeField]
        protected string codeName;
        public string CodeName => codeName;
        [SerializeField]
        protected List<ActionTestData> actionsData = new List<ActionTestData>();
        public List<ActionTestData> ActionsData => actionsData;
    }

    [System.Serializable]
    public class ActionTestData
    {
        public string CodeName;
        public float Duration;
        public float MoveSpeed;

        public Hiker.Idle.Condition Condition;
    }
}
