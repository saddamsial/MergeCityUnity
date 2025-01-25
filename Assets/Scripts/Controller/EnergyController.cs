using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnergyController 
{
    // Start is called before the first frame update
    private float currentEnergy;
    private int initEnergy;
    private int maxEnergy;
    private float recoverSpeed;

    public static Action OnOverEnergy = delegate { };
    public static Action<float> OnUpdateEnergy = delegate { };

public float CurrentEnergy { get => currentEnergy; set => currentEnergy = value; }
    public int InitEnergy { get => initEnergy; set => initEnergy = value; }
    public int MaxEnergy { get => maxEnergy; set => maxEnergy = value; }

    public void Init()
    {
        initEnergy = ConfigManager.Instance.EnergyConfig.BaseEnergy;
        Debug.Log("init energy :" + ConfigManager.Instance.EnergyConfig.BaseEnergy);
        maxEnergy = ConfigManager.Instance.EnergyConfig.MaxEnergy;
        recoverSpeed = ConfigManager.Instance.EnergyConfig.RecoverSpeed;
        currentEnergy = initEnergy;
    }
    // Update is called once per frame
    public void UpdateEnergy(int energy)
    {
        currentEnergy += energy;
        Debug.Log("current energy :" + currentEnergy);
        currentEnergy = Mathf.Clamp((float)currentEnergy, 0f, (float)maxEnergy);
        OnUpdateEnergy?.Invoke(currentEnergy);
        if (currentEnergy <= 0)
        {
            OnOverEnergy?.Invoke();
            
        }
    }
}
