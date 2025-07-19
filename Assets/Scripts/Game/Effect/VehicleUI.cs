using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using TMPro;
using UnityEngine.UI;

[UIPath("UI/InGame/VehicleUI", false)]
public class VehicleUI : InGameFloatingUI
{

    [SerializeField]
    private TextMeshProUGUI VehicleBuffText;



    public void SetText(int buffvalue)
    {
        VehicleBuffText.text = $"↑{buffvalue}%";
    }
}
