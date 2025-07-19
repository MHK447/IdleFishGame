using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class HudTopCurrency : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI EnergycoinText;

    [SerializeField]
    private TextMeshProUGUI EnergyCoinTimeText;

    [SerializeField]
    private GameObject EnergyRoot;

    [SerializeField]
    private TextMeshProUGUI CashText;

    [SerializeField]
    private Button CashBtn;

    [SerializeField]
    private Button EnergyBtn;



    private CompositeDisposable disposables = new CompositeDisposable();


    void Awake()
    {
        CashBtn.onClick.AddListener(OnClickCash);
        EnergyBtn.onClick.AddListener(OnClickEnergy);
    }

    public void OnClickCash()
    {
        GameRoot.Instance.UISystem.OpenUI<PageShop>(page => page.Init());
    }

    public void OnClickEnergy()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupPurchaseLightning>(popup => popup.Init());
    }

    void OnEnable()
    {
        disposables.Clear();

        GameRoot.Instance.UserData.Cash.Subscribe(x =>
        {

            CashText.text = $"{x}";
        }).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }
}
