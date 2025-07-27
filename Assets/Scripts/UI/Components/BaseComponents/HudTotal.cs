using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;    



[UIPath("UI/Page/HudTotal", true)]
public class HudTotal : UIBase
{
    [SerializeField]
    private Button Upgradebtn;
    [SerializeField]
    private Button AquariumBtn;

    

    protected override void Awake()
    {
        base.Awake();

        Upgradebtn.onClick.AddListener(OnClickUpgradeBtn);
        AquariumBtn.onClick.AddListener(OnClickAquariumBtn);
    }

    public void OnClickUpgradeBtn()
    {

    }


    public void OnClickAquariumBtn()
    {

    }

    public void Init()
    {

    }
}
