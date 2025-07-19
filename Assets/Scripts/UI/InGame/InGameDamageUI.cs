using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;

[UIPath("UI/InGame/InGameDamageUI", false,true)]
public class InGameDamageUI : InGameFloatingUI
{
    [SerializeField]
    private Text DamageText;

    public void SetDamage(int damage)
    {
        DamageText.text = damage.ToString();
    }


    public void CloseAction()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, false);
    }
}
