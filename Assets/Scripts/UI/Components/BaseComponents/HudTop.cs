using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using BanpoFri;

public class HudTop : MonoBehaviour
{
    [SerializeField]
    private Text CashText;

    [SerializeField]
    private Text MoneyText;

    [SerializeField]
    private Text MaterialText;

    [SerializeField]
    private Text StageRewardText;

    [SerializeField]
    private Text EnergyMoneyText;

    private void Awake()
    {
        SyncData();
    }


    private void SyncData()
    {
        if (CashText != null)
        {
            CashText.text = GameRoot.Instance.UserData.Cash.Value.ToString();
            GameRoot.Instance.UserData.HUDCash.Subscribe(x =>
            {
                CashText.text = x.ToString();
            }).AddTo(this);

        }
        if (MoneyText != null)
        {
            MoneyText.text = Utility.CalculateMoneyToString(GameRoot.Instance.UserData.CurMode.Money.Value);

            GameRoot.Instance.UserData.HUDMoney.Subscribe(x =>
            {
                MoneyText.text = Utility.CalculateMoneyToString(GameRoot.Instance.UserData.CurMode.Money.Value);
            }).AddTo(this);
        }

    }       
}
