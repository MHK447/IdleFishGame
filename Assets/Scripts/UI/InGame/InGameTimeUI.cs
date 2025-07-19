using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;

[UIPath("UI/InGame/InGameTimeUI", false, true)]
public class InGameTimeUI : InGameFloatingUI
{
    [SerializeField]
    private Text TimeText;


    public void SetTime(int time)
    {
        TimeText.text = Utility.GetTimeStringFormattingShort(time);
    }

}
