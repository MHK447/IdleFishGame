using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using TMPro;
using UnityEngine.UI;



[UIPath("UI/Popup/PopupPurchaseLightning")]
public class PopupPurchaseLightning : UIBase
{
    [SerializeField]
    private TextMeshProUGUI PriceText;

    [SerializeField]
    private Button PurchaseBtn;

    private int PriceCount = 60;


    protected override void Awake()
    {
        base.Awake();
        PurchaseBtn.onClick.AddListener(OnClickPurchase);
    }

    public void OnClickPurchase()
    {
        if(GameRoot.Instance.UserData.Cash.Value >= PriceCount)
        {
            GameRoot.Instance.UserData.Cash.Value -= PriceCount;

            GameRoot.Instance.UserData.Energycoin.Value = 100;

            Hide();
        }



    }



    public void Init()
    {

    }
    
}
