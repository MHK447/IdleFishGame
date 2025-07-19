using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using TextOutline;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupOption")]
public class PopupOption : UIBase
{
    [Space(1.5f)]
    [Header("Option")]
    [SerializeField]
    private ToggleController SoundToggle;
    [SerializeField]
    private ToggleController MusicToggle;

    [SerializeField] private TMP_Dropdown langDropdown;

    [SerializeField]
    private Button restoreBtn;

    // ATT 디버그 관련 추가
    [Header("ATT Debug (iOS Only)")]
    [SerializeField] 
    private Button attTestButton; // Inspector에서 할당 필요
    [SerializeField] 
    private TMPro.TextMeshProUGUI attStatusText; // Inspector에서 할당 필요

    protected override void Awake()
    {
        base.Awake();

        SoundToggle.Init(GameRoot.Instance.UserData.Effect);
        MusicToggle.Init(GameRoot.Instance.UserData.Bgm);

        SoundToggle.setToggleListener(OnClickEffect);
        MusicToggle.setToggleListener(OnClickBGM);

        var list = new List<string>();
        for (Config.Language lang = Config.Language.en; lang <= Config.Language.ja; lang++)
        {
            list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{lang.ToString()}"));
        }
        
        langDropdown.AddOptions(list);

        langDropdown.onValueChanged.AddListener(OnClickSelectLang);

        //versionInfo.text = $"version : {Application.version}";
    }

    public override void OnShowBefore()
    {
        SetLang();
        base.OnShowBefore();
    }

    private void OnClickSelectLang(int index)
    {
        Config.Language curLang = GameRoot.Instance.UserData.Language;
        switch (index)
        {
            case 0: curLang = Config.Language.en; break;
            case 1: curLang = Config.Language.ko; break;
            case 2: curLang = Config.Language.ja; break;
            default: curLang = Config.Language.en; break;
        }

        GameRoot.Instance.UserData.Language = curLang;

        GameRoot.Instance.WaitEndFrameCallback(() =>
        {
            foreach (var ls in LocalizeString.Localizelist)
            {
                if (ls != null)
                {
                    ls.RefreshText();
                }
            }
        
            var list = GameRoot.Instance.UISystem.RefreshComponentList;
            foreach (var ls in list)
                ls.RefreshText();

        });
    }

    string EscapeURL(string url)
    {
        return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }

    //public void SetBonusGem()
    //{
    //    TpUtility.SetActiveCheck(bonusGem, GameRoot.Instance.UserData.GetRecordValue(Config.RecordKeys.HasLogined) <= 0);
    //    // bonusGemText.text = $"+{Tables.Instance.GetTable<Define>().GetData("login_bonus_gem").value}";
    //}

    //private void OnClickVibration(bool isOn)
    //{
    //    GameRoot.Instance.UserData.Vib = isOn;
    //    GameRoot.Instance.UserData.Save();
    //}

    private void OnClickEffect(bool isOn)
    {
        GameRoot.Instance.UserData.Effect = isOn;
        SoundPlayer.Instance.EffectSwitch(isOn);
        GameRoot.Instance.UserData.Save();
    }

    private void OnClickBGM(bool isOn)
    {
        GameRoot.Instance.UserData.Bgm = isOn;
        SoundPlayer.Instance.BgmSwitch(isOn);
        GameRoot.Instance.UserData.Save();
    }

    private void SetLang()
    {
       int idx = 0;
       switch (GameRoot.Instance.UserData.Language)
       {
           case Config.Language.en: idx = 0; break;
           case Config.Language.ko: idx = 1; break;
           case Config.Language.ja: idx = 2; break;
           default: idx = 0; break;
       }

       langDropdown.value = idx;
    }


    void OnClickRestore()
    {
       GameRoot.Instance.Loading.Show(false);
    //    GameRoot.Instance.InAppPurchaseManager.RestorePurchase((result) =>
    //    {
    //        if (result == InAppPurchaseManager.Result.Success)
    //        {
    //            GameRoot.Instance.ShopSystem.NoAds.Value = true;
    //        }

    //        var stringKey = result == InAppPurchaseManager.Result.Success ? "str_restore_success" : "str_restore_fail";
    //        GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup =>
    //        {
    //            popup.Show(Tables.Instance.GetTable<Localize>().GetString("str_restore"), Tables.Instance.GetTable<Localize>().GetString(stringKey));
    //        });

    //        GameRoot.Instance.Loading.Hide(true);
    //    });
    }

    //#region Lang
    // private void SetLang()
    // {
    //    langDropdown.value = (int)GameRoot.Instance.UserData.Language;
    // }

    // private void OnClickSelectLang(int index)
    // {
    //    Config.Language curLang = Config.Language.en;
    //    switch (index)
    //    {
    //        case 0: curLang = Config.Language.en; break;
    //        case 1: curLang = Config.Language.ko; break;
    //        case 2: curLang = Config.Language.ja; break;
    //        default: curLang = Config.Language.en; break;
    //    }

    //    GameRoot.Instance.UserData.Language = curLang;

    //    foreach (var ls in LocalizeString.Localizelist)
    //    {
    //        if (ls != null)
    //        {
    //            ls.RefreshText();
    //        }
    //    }

    //    var list = GameRoot.Instance.UISystem.RefreshComponentList;
    //    foreach (var ls in list)
    //        ls.RefreshText();
    // }
    // #endregion


}

