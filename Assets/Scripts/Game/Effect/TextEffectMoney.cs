using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;    

[EffectPath("Effect/TextEffectMoney", false, true)]
public class TextEffectMoney : Effect
{
    [SerializeField]
    private TextMeshProUGUI MoneyText;

    public void SetText(System.Numerics.BigInteger value)
    {
        GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, value);
        MoneyText.text = $"+{ ProjectUtility.CalculateMoneyToString(value)}";
    }
}
