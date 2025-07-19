using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;

[UIPath("UI/InGame/InGameHpUI", false , true)]
public class InGameHpUI : InGameFloatingUI
{
    [SerializeField]
    private Slider SliderValue;


    private int CurHp = 0;

    private int StartHp = 0;
        
    public void Set(int hp)
    {
        SliderValue.value = 1f;
        StartHp = CurHp = hp;
    }

    public void SetSliderValue(int damage)
    {
        CurHp -= damage;
        SliderValue.value = (float)CurHp / (float)StartHp;
    }

}
