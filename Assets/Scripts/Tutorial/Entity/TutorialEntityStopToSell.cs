using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;


public class TutorialEntityStopToSell : TutorialEntity
{
    [SerializeField]
    private Text Text;

    public override void StartEntity()
    {
        base.StartEntity();

        Text.text = Tables.Instance.GetTable<Localize>().GetString("str_tutorial_stop_desc");

        GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, 500);

        GameRoot.Instance.WaitTimeAndCallback(1f, () => { Done(); });
    }

}
