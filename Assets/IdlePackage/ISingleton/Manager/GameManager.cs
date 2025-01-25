using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public class GameManager : MonobehaviourCustom
    {
        #region Singleton

        private static GameManager instance;
        public static GameManager Instance { get => instance; set => instance = value; }

        #endregion

        #region Properties

        [SerializeField] bool IsInitAwake;
        [SerializeField] bool IsInitStart;
        [SerializeField] bool IsInitSuccess;

        public List<GameObject> lstISingleton = new List<GameObject>();

        #endregion

        #region Monobehaviour

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
                instance.InitLoadFirst();
            }
        }

        private void Start()
        {
            if(instance != null)
            {
                instance.InitLoadLast();
            }
        }

        #endregion

        #region Init & Load Manager

        public bool InitLoadFirst()
        {
            if(lstISingleton == null)
            {
                DebugError(name, "GameManager Init Load First Error List ISingleton is Null !");
                return false;
            }

            for (int i = 0; i < lstISingleton.Count; i++)
            {
                var Ising = lstISingleton[i].GetComponent<ISingleton>();

                if(Ising == null)
                {
                    DebugError(name, lstISingleton[i].name + " Not Found ISingleton Script !");
                    return false;
                }

                Ising.InitAwake();
            }

            IsInitAwake = true;
            return true;
        }

        public bool InitLoadLast()
        {
            if (lstISingleton == null)
            {
                DebugError(name, "GameManager Init Load First Error List ISingleton is Null !");
                return false;
            }

            for (int i = 0; i < lstISingleton.Count; i++)
            {
                var Ising = lstISingleton[i].GetComponent<ISingleton>();

                if (Ising == null)
                {
                    DebugError(name, lstISingleton[i].name + " Not Found ISingleton Script !");
                    return false;
                }

                Ising.InitStart();
            }

            IsInitStart = true;
            IsInitSuccess = IsInitAwake & IsInitStart;
            return true;
        }

        #endregion

        #region Function GameManager

        #endregion
    }
}


