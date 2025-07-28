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
    private TextMeshProUGUI MoneyText;

    [SerializeField]
    private GameObject MoneyRoot;

    [SerializeField]
    private TextMeshProUGUI CashText;

    [SerializeField]
    private Button CashBtn;


    private CompositeDisposable disposables = new CompositeDisposable();


    void Awake()
    {
       //CashBtn.onClick.AddListener(OnClickCash);
    }


    void OnEnable()
    {
        disposables.Clear();

        GameRoot.Instance.UserData.Money.Subscribe(x =>
        {
            MoneyText.text = ProjectUtility.CalculateMoneyToString(x);
        }).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Clear();
    }
}
