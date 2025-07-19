using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;

[UIPath("UI/InGame/CooltimeProgress", false)]
public class CooltimeProgress : InGameFloatingUI
{
    [SerializeField]
    private Image Progress;

    public void SetValue(float value)
    {
        UpdatePos();
        Progress.fillAmount = value;
    }

   
}
