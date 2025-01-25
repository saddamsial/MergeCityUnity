using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public class BuildingManager : MonobehaviourCustom, ISingleton
    {
        #region Singleton

        private static BuildingManager instance;
        public static BuildingManager Instance { get => instance; set => instance = value; }

        public void InitAwake()
        {
            if (Instance == null)
            {
                Instance = this;
                DebugLog(name, "Init Instance Success", "Green", "Init Log");
                InitLoadSceneObject();
            }
        }

        public void InitStart()
        {

        }

        #endregion

        #region Properties
        private string PathPrefabs = "BuildingPrefabs/";
        public Dictionary<string, BuildingBase> ListBuilding = new Dictionary<string, BuildingBase>();
        public GameObject ContainerBuilding;

        public List<BuildObject> ListBuildingObjectScene = new List<BuildObject>();
        #endregion

        #region Init Load

        public void InitLoadSceneObject()
        {
            var temp = ContainerBuilding.GetComponentsInChildren<BuildObject>();
            if (temp != null && temp.Length > 0)
            {
                ListBuildingObjectScene = ConverToListFromArray(temp);
                foreach (var item in ListBuildingObjectScene)
                {
                    AddBuildingObjectScene(item);
                }
            }
        }

        #endregion

        #region Monobehaviour

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                CreateBuilding("GOLD", "BuildExample", new Vector3(10, 0, 0));
            }
        }

        #endregion

        #region Function Init Building Manager

        public void CreateBuilding(string CodeNameBuilding, string CodeNamePrefabs, Vector3 Pos)
        {

            //Create Build Ojbect
            BuildObject buildObject = CreateBuildingObject(CodeNamePrefabs, Pos);
            if (buildObject == null)
            {
                DebugLog(name, "Create Building Object Error", "Red", "Error Create");
                return;
            }

            //Create Building Base
            BuildingBase buildingBase = GetBuildingBase(CodeNameBuilding);
            if (buildingBase == null) //Not Exists
            {
                buildingBase = new BuildingBase();
                buildingBase.InitBuildingBase(CodeNameBuilding);

                AddBuilding(buildingBase);
            }

            buildingBase.InitEventBuilding(buildObject);
            buildingBase.AddBuildObject(buildObject);
            //buildObject.InitBuildSlots(buildObject.ObjBuildSlot);
        }

        public void AddBuildingObjectScene(BuildObject buildObject)
        {
            //Create Building Base
            BuildingBase buildingBase = GetBuildingBase(buildObject.CodeNameBuilding);
            if (buildingBase == null) //Not Exists
            {
                buildingBase = new BuildingBase();
                buildingBase.InitBuildingBase(buildObject.CodeNameBuilding);

                AddBuilding(buildingBase);
            }

            buildingBase.InitEventBuilding(buildObject);
            buildingBase.AddBuildObject(buildObject);
            //buildObject.InitBuildSlots(buildObject.ObjBuildSlot);
        }

        public bool IsHasBuilding(string CodenameBuilding)
        {
            return ListBuilding.ContainsKey(CodenameBuilding);
        }

        public void AddBuilding(BuildingBase buildingBase)
        {
            ListBuilding.Add(buildingBase.BuildCodeName, buildingBase);
        }

        public BuildingBase GetBuildingBase(string CodenameBuilding)
        {
            if (ListBuilding.ContainsKey(CodenameBuilding))
            {
                return ListBuilding[CodenameBuilding];
            }

            return null;
        }

        public BuildObject CreateBuildingObject(string CodeNamePrefabsBuilding, Vector3 Pos)
        {
            GameObject o = Resources.Load<GameObject>(PathPrefabs + CodeNamePrefabsBuilding);
            if (o == null)
            {
                DebugLog(name, "Not Found Prefabs at Path: " + PathPrefabs + CodeNamePrefabsBuilding, "Red", "Error Prefabs");
                return null;
            }

            GameObject oBuilding = Instantiate(o, ContainerBuilding.transform);
            if (oBuilding == null)
            {
                DebugLog(name, "Init Object Error", "Red", "Error");
                return null;
            }

            BuildObject result = oBuilding.GetComponent<BuildObject>();
            if (result == null)
            {
                DebugLog(name, "Not Found BuildObject(S)", "Red", "Error");
                return null;
            }

            result.SetPositionBuildObject(Pos);
            return result;
        }

        #endregion

        #region Pre Book

        //public BuildObject PreBookBuildingAPI(string CodeNameBuilding, UnitBehavior unitBehavior, System.Action ToDoCallBack, TypeSlot typeSlot = TypeSlot.None)
        public BuildObject PreBookBuildingAPI(string CodeNameBuilding, UnitBehavior unitBehavior, string typeSlot = null)
        {
            BuildingBase buildingBase = GetBuildingBase(CodeNameBuilding);

            if (buildingBase == null)
            {
                DebugLog(name, "Not Exists Building: " + CodeNameBuilding, "Red", "Error");
                return null;
            }

            BuildObject buildObject = null;

            if (!string.IsNullOrEmpty(typeSlot))
            {
                buildObject = buildingBase.GetBuildingSlot(unitBehavior, typeSlot);
            }
            else
            {
                buildObject = buildingBase.GetBuildingSlot(unitBehavior);
            }

            if (buildObject == null)
            {
                DebugLog(name, "Not Exists BuildObject: " + CodeNameBuilding, "Red", "Error");
                return null;
            }

            //buildObject.RegisterPreBookEvent(ToDoCallBack);
            return buildObject;
        }

        #endregion

        #region Queue

        //public TypeJoinQueue JoinQueue(BuildObject buildObject, UnitBehavior unitBehavior, TypeSlot typeSlot = TypeSlot.None)
        //{
        //    return buildObject.JoinQueue(unitBehavior, typeSlot);
        //}

        #endregion

        #region Is Condition

        public bool IsStartAction(BuildObject buildObject, int Count = 1, string typeSlot = null)
        {
            return buildObject.IsStartActivity(Count, typeSlot);
        }

        #endregion

        #region Function To Do Event Temp
        public static void ToDoEvent()
        {

        }
        #endregion
    }
}