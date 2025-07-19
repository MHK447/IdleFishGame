using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;
using UniRx;

public class ShopProductComponent : MonoBehaviour
{
    [SerializeField]
    private ShopSystem.ProductShopType Type;

    [SerializeField]
    private Button PurchaseBtn;

    [SerializeField]
    private TextMeshProUGUI PriceText;

    [SerializeField]
    private List<TextMeshProUGUI> RewardValueText = new List<TextMeshProUGUI>();

    [SerializeField]
    private GameObject CoolTimeRoot;

    [SerializeField]
    private TextMeshProUGUI CoolTimeText;

    private InAppPurchaseManager purchaseManager;

    private CompositeDisposable disposables = new CompositeDisposable();

    private string ProductId = "";


    void Awake()
    {
        PurchaseBtn.onClick.AddListener(OnClickPurchaseBtn);
    }

    public void Init()
    {
        purchaseManager = GameRoot.Instance.GetInAppPurchaseManager;

        if (purchaseManager != null && purchaseManager.IsInitialized)
        {
            var td = Tables.Instance.GetTable<ShopProduct>().GetData((int)Type);

            if (td != null)
            {

                ProductId = td.product_id;
                string price = purchaseManager.GetLocalizedPrice(td.product_id);

                if (PriceText != null)
                    PriceText.text = price;

                for (int i = 0; i < RewardValueText.Count; i++)
                {
                    RewardValueText[i].text = td.value[i].ToString();
                }

                disposables.Clear();

                GameRoot.Instance.ShopSystem.FreeAdRemindTime.Subscribe(x =>
                {
                    TimeCheck(x);
                }).AddTo(disposables);

                CoolTimeCheck();
            }
        }
    }


    public void CoolTimeCheck()
    {
        if (CoolTimeRoot != null)
            ProjectUtility.SetActiveCheck(CoolTimeRoot, false);

        switch (Type)
        {
            case ShopSystem.ProductShopType.AdGem:
                {
                    var count = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdGemCount);
                    ProjectUtility.SetActiveCheck(CoolTimeRoot, count > 0);
                }
                break;
            case ShopSystem.ProductShopType.FreeGem:
                {
                    var count = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.FreeGemCount);
                    ProjectUtility.SetActiveCheck(CoolTimeRoot, count > 0);
                }
                break;
        }
    }


    public void TimeCheck(int time)
    {
        if (CoolTimeText == null) return;

        CoolTimeText.text = ProjectUtility.GetTimeStringFormattingShort(time);

        if (time <= 0)
        {
            CoolTimeCheck();
        }
    }


    void OnDestroy()
    {
        disposables.Clear();
    }

    public void OnClickPurchaseBtn()
    {
        var td = Tables.Instance.GetTable<ShopProduct>().GetData((int)Type);

        if (td == null)
        {
            return;
        }


        switch (Type)
        {
            case ShopSystem.ProductShopType.AdGem:
                {
                    GameRoot.Instance.GetAdManager.ShowRewardedAd((() =>
                    {
                        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.AdGemCount, 1);
                        
                        // 실제 잼 지급
                        for (int i = 0; i < td.reward_type.Count; i++)
                        {
                            GameRoot.Instance.ShopSystem.RewardPay(td.reward_type[i], td.reward_idx[i], td.value[i]);
                        }
                        
                        CoolTimeCheck();
                    }));

                }
                break;
            case ShopSystem.ProductShopType.FreeGem:
                {
                    GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.FreeGemCount, 1);
                    
                    // 실제 잼 지급
                    for (int i = 0; i < td.reward_type.Count; i++)
                    {
                        GameRoot.Instance.ShopSystem.RewardPay(td.reward_type[i], td.reward_idx[i], td.value[i]);
                    }
                    
                    CoolTimeCheck();
                }
                break;
            default:
                {
                    if (purchaseManager != null && purchaseManager.IsInitialized)
                    {
                        GameRoot.Instance.Loading.Show(true);

                        purchaseManager.PurchaseProduct(ProductId, (result, message) =>
                        {
                            GameRoot.Instance.Loading.Hide(true);

                            if (result == InAppPurchaseManager.Result.Success)
                            {
                                var td = Tables.Instance.GetTable<ShopProduct>().GetData((int)Type);

                                if (td != null)
                                {
                                    for (int i = 0; i < td.reward_type.Count; i++)
                                    {
                                        GameRoot.Instance.ShopSystem.RewardPay(td.reward_type[i], td.reward_idx[i], td.value[i]);
                                    }
                                }
                            }
                            else
                            {
                                // 구매 실패 메시지 표시
                                // GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => {
                                //     popup.Show("구매 실패", message);
                                // });
                            }
                        });
                    }
                    break;
                }
        }
    }
}
