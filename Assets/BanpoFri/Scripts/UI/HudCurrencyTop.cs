using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;


public class HudCurrencyTop : MonoBehaviour
{
    public Text CashText;

    public Text MoneyText;

    public Image CashImg;

    public Image MoneyImg;



    public Transform GetImageTr(int rewardtype, int rewardidx)
    {
        switch (rewardtype)
        {
            case (int)Config.RewardType.Currency:
                {
                    switch (rewardidx)
                    {
                        case (int)Config.CurrencyID.Cash:
                            return CashImg.transform;
                        case (int)Config.CurrencyID.Money:
                            return MoneyImg.transform;
                    }
                }
                break;
        }


        return null;
    }


    // private void Awake()
    // {
    //     SyncData();
    // }


}
