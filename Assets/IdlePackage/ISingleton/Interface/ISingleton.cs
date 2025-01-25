using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public interface ISingleton
    {
        public void InitAwake();
        public void InitStart();
    }
}
