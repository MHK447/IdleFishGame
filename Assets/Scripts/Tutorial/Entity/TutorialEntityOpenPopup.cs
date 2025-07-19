using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;

public enum OpenPopupType
{
    ToastMessage,
    TowerToastMessage,
    ArtifactToastMessage,

}

public class TutorialEntityOpenPopup : TutorialEntity
{
    [SerializeField]
    private OpenPopupType Type;

    public override void StartEntity()
    {
        base.StartEntity();


        switch(Type)
        {
            case OpenPopupType.ToastMessage:
                {
                    GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => popup.Show("", Tables.Instance.GetTable<Localize>().GetString("str_tutorial_char")));
                }
                break;
            case OpenPopupType.TowerToastMessage:
                {
                    GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => popup.Show("", Tables.Instance.GetTable<Localize>().GetString("str_toast_tutorial_tower_open")));
                }
                break;
            case OpenPopupType.ArtifactToastMessage:
                {
                    GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup => popup.Show("", Tables.Instance.GetTable<Localize>().GetString("str_desc_tutorial_enhance_complete")));
                }
                break;
        }

        Done();
    }
}
