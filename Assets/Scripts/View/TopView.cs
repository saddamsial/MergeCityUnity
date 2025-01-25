using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TopView : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private TextMeshProUGUI currentEnergy;

    [SerializeField]
    private TextMeshProUGUI maxEnergy;

    [SerializeField]
    private GameManager gameManager;
    private void OnEnable()
    {
        EnergyController.OnUpdateEnergy += UpdateEnergy;
    }
    private void OnDisable()
    {
        EnergyController.OnUpdateEnergy -= UpdateEnergy;
    }
    private void UpdateEnergy(float newEnergy)
    {
        currentEnergy.text = newEnergy.ToString();
    }

    void Start()
    {
        currentEnergy.text = gameManager.energyController.CurrentEnergy.ToString();
        maxEnergy.text = gameManager.energyController.MaxEnergy.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
