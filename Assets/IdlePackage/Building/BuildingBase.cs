using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public class BuildingBase
    {
        #region Properties
        public string BuildCodeName;
        public List<BuildObject> buildObjects = new List<BuildObject>();
        private int CurrentInitID = 0;

        //Event
        public System.Action<BuildObject> QueueCallBackEvent;


        #endregion

        #region Function Init

        public void InitBuildingBase(string CodeNameBuilding)
        {
            BuildCodeName = CodeNameBuilding;
            buildObjects = new List<BuildObject>();
        }

        public void InitEventBuilding(BuildObject buildObject)
        {
            //Init Event
            buildObject.UpdateBuildingBaseEvent += UpdateFreeBuildObject;
        }

        #endregion

        #region Function BuildObject Manager

        public void RemoveBuildObject(BuildObject buildObject)
        {
            if (buildObjects.Contains(buildObject))
            {
                buildObjects.Remove(buildObject);
            }
        }

        public void RemoveBuildObject(string codeNameBuildOject)
        {
            foreach (var item in buildObjects)
            {
                if (item.CodeName == codeNameBuildOject)
                {
                    buildObjects.Remove(item);
                    return;
                }
            }
        }

        public void AddBuildObject(BuildObject buildObject)
        {
            if (!buildObjects.Contains(buildObject))
            {
                buildObject.CodeNameBuilding = BuildCodeName;
                buildObject.CodeName = BuildCodeName + CurrentInitID;

                buildObjects.Add(buildObject);

                CurrentInitID++;
            }
        }

        #endregion

        #region Funtion API

        public BuildObject GetBuildingSlot(UnitBehavior unitBehavior)
        {
            BuildObject temp = null;
            BuildObject tempB = null;

            foreach (var item in buildObjects)
            {
                if (item.IsHasSlot())
                {
                    temp = GetBuildObjectNear(temp, item, unitBehavior);
                }
                tempB = GetBuildObjectNear(tempB, item, unitBehavior);
            }

            return (temp != null) ? temp : tempB;
        }
        public BuildObject GetBuildingSlot(UnitBehavior unitBehavior, string typeSlot)
        {
            BuildObject temp = null;
            BuildObject tempB = null;

            foreach (var item in buildObjects)
            {
                if (item.IsHasSlot(typeSlot))
                {
                    temp = GetBuildObjectNear(temp, item, unitBehavior);
                }
                tempB = GetBuildObjectNear(tempB, item, unitBehavior);
            }

            return (temp != null) ? temp : tempB;
        }

        public BuildObject GetBuildObjectNear(BuildObject a, BuildObject b, UnitBehavior unitBehavior)
        {
            if (a == null || b == null) return (a == null) ? b : a;

            float disA = Vector3.Distance(a.InPosition, unitBehavior.transform.position);
            float disB = Vector3.Distance(b.InPosition, unitBehavior.transform.position);

            return (disA < disB) ? a : b;
        }

        public BuildObject GetBuildObject(string CodeNameBuildObjet)
        {
            foreach (var item in buildObjects)
            {
                if (item.CodeName == CodeNameBuildObjet)
                {
                    return item;
                }
            }

            return null;
        }

        #endregion

        #region Function Update BuildingBase

        private void UpdateFreeBuildObject(BuildObject buildObject)
        {
            QueueCallBackEvent?.Invoke(buildObject);
        }

        #endregion
    }
}

