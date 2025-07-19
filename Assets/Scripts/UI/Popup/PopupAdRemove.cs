using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;


[UIPath("UI/Popup/PopupAdRemove")]
public class PopupAdRemove : UIBase
{
    [SerializeField]
    private TextMeshProUGUI PriceText;

    [SerializeField]
    private Button PurchaseBtn;

    private InAppPurchaseManager purchaseManager;

    protected override void Awake()
    {
        base.Awake();
        PurchaseBtn.onClick.AddListener(OnClickReward);

        // InAppPurchaseManager 찾기
        purchaseManager = GameRoot.Instance.GetInAppPurchaseManager;
    }

    public void Init()
    {
        // 상품 가격 표시
        if (purchaseManager != null && purchaseManager.IsInitialized)
        {
            string price = purchaseManager.GetLocalizedPrice(InAppPurchaseManager.ProductIDs.REMOVE_ADS);
            PriceText.text = price;
        }
    }


    public void OnClickReward()
    {
        // 인앱 결제 시작
        if (purchaseManager != null && purchaseManager.IsInitialized)
        {
            GameRoot.Instance.Loading.Show(true);

            purchaseManager.PurchaseProduct(InAppPurchaseManager.ProductIDs.REMOVE_ADS, (result, message) =>
            {
                GameRoot.Instance.Loading.Hide(true);

                if (result == InAppPurchaseManager.Result.Success)
                {
                    // 구매 성공
                    GameRoot.Instance.ShopSystem.IsVipProperty.Value = true;

                    // 구매 성공 메시지 표시
                    // GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => {
                    //     popup.Show("구매 완료", "광고 제거 상품 구매가 완료되었습니다.");
                    // });

                    Hide();
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
    }
}
