using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;


[EffectPath("Effect/TextEffectMoney",false , true)]
public class TextEffectMoney : Effect
{
    [SerializeField]
    private Text MoneyText;

    public void SetText(System.Numerics.BigInteger value)
    {
        MoneyText.text = ProjectUtility.CalculateMoneyToString(value);
    }
}
