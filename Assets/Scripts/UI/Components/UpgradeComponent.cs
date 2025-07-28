using BanpoFri;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;


public class UpgradeComponent : MonoBehaviour
{
    [SerializeField]
    private int UpgradeIdx;

    [SerializeField]
    private TextMeshProUGUI UpgradeValueText;

    [SerializeField]
    private Button UpgradeBtn;

    [SerializeField]
    private TextMeshProUGUI PriceText;

    private System.Numerics.BigInteger Price;

    private CompositeDisposable disposable  = new CompositeDisposable();


    private void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnClickUpgrade);
    }


    void OnEnable()
    {
        SetInfo();
    }


    public void SetInfo()
    {
        var td = Tables.Instance.GetTable<UpgradeInfo>().GetData(UpgradeIdx);

        if (td != null)
        {
            var level = GameRoot.Instance.UserData.Upgradedata[UpgradeIdx].Upgradelevel;

            Price = GameRoot.Instance.UpgradeSystem.GetUpgradeCost(level, td.base_upgrade_cost, (float)td.inceease_upgrade_cost * 0.01f);

            PriceText.text = ProjectUtility.CalculateMoneyToString(Price);

            UpgradeValueText.text = Tables.Instance.GetTable<Localize>().GetFormat(td.str_desc, GameRoot.Instance.UpgradeSystem.GetUpgradeValue((UpgradeSystem.UpgradeType)UpgradeIdx));


            disposable.Clear();

            GameRoot.Instance.UserData.Money.Subscribe(x=> {

                UpgradeBtn.interactable = x >= Price;
            }).AddTo(disposable);
        }
    }

    public void OnClickUpgrade()
    {
        if (GameRoot.Instance.UserData.Money.Value >= Price)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency , (int)Config.CurrencyID.Money , -Price);

            GameRoot.Instance.UserData.Upgradedata[UpgradeIdx].Upgradelevel++;

            SetInfo();
        }

    }


    void OnDestroy()
    {
        disposable.Clear();
    }
}
