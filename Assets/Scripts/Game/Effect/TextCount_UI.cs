using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;

[UIPath("UI/InGame/TextCount_UI", false)]
public class TextCount_UI : InGameFloatingUI
{
    [SerializeField]
    private Text TextCount;


    public void SetText(int count , int maxcount)
    {
        TextCount.text = $"{count}/{maxcount}";
    }



}
