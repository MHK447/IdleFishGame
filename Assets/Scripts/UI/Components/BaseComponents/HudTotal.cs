using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;    
using UniRx;


[UIPath("UI/Page/HudTotal", true)]
public class HudTotal : UIBase
{
    [SerializeField]
    private Button Upgradebtn;
    [SerializeField]
    private Button AquariumBtn;

    [SerializeField]
    private TextMeshProUGUI DepthText;

    

    protected override void Awake()
    {
        base.Awake();

        Upgradebtn.onClick.AddListener(OnClickUpgradeBtn);
        AquariumBtn.onClick.AddListener(OnClickAquariumBtn);

        GameRoot.Instance.PlayerSystem.SeaDepthProperty.Subscribe(x =>
        {
            DepthText.text = x < 0 ? "0.00m" : $"{x.ToString("F2")}m";
        }).AddTo(this);
    }

    public void OnClickUpgradeBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupUpgrade>();

    }


    public void OnClickAquariumBtn()
    {

    }
}
