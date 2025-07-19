using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;

[UIPath("UI/Popup/PopupToastmessage")]
public class PopupToastmessage : UIBase
{
    [SerializeField]
    private TextMeshProUGUI desc;

    private int RewardType;
    private int RewardIdx;
    private int RewardCount;

    public void Show(string _title, string _desc)
    {
        Show(_title, _desc, "Renovate_Icon_Alarm_Warning");
    }

    public void Show(string _title, string _desc, string _renovateImgName)
    {
        //icon.sprite = Config.Instance.GetRenovateImg(_renovateImgName);
        desc.text = _desc;


        GameRoot.Instance.WaitTimeAndCallback(3f, () => { Hide(); ProjectUtility.SetActiveCheck(this.gameObject, false); });
    }

    public override void CustomSortingOrder()
    {
        base.CustomSortingOrder();
        transform.GetComponent<Canvas>().sortingOrder = 32767;
    }

    public void ShowNewStage()
    {
        //TpUtility.SetActiveCheck(icon.gameObject, false);
        //TpUtility.SetActiveCheck(title.gameObject, true);

        //var stage = GameRoot.Instance.UserData.CurMode.StageData.StageIdx;
        //var td = Tables.Instance.GetTable<StageSet>().GetData(stage);

        //icon.sprite = Config.Instance.GetRenovateImg(td.stage_icon);
        //title.text = string.Format(Tables.Instance.GetTable<Localize>().GetString("str_welcome_title"),
        //                            Tables.Instance.GetTable<Localize>().GetString(td.stage_name));
        //desc.text = Tables.Instance.GetTable<Localize>().GetString("str_welcome_desc");

        //var prev_td = Tables.Instance.GetTable<StageSet>().GetData(stage - 1);
        //TpUtility.SetActiveCheck(RewardObj.gameObject, prev_td != null && prev_td.clear_reward_idx > 0);

        //if (prev_td != null && prev_td.clear_reward_idx > 0)
        //{
        //    RewardIcon.sprite = ProjectUtility.GetRewardIconImg((Config.RewardType)prev_td.clear_reward_type, prev_td.clear_reward_idx);
        //    RewardValue.text = $"x{prev_td.clear_reward_count}";

        //    RewardType = prev_td.clear_reward_type;
        //    RewardIdx = prev_td.clear_reward_idx;
        //    RewardCount = prev_td.clear_reward_count;
        //}
    }

    public void ShowPrepareStage()
    {
        //TpUtility.SetActiveCheck(icon.gameObject, true);

        //icon.sprite = Config.Instance.GetRenovateImg("Renovate_Icon_Stage_Prepare");
        desc.text = Tables.Instance.GetTable<Localize>().GetString("str_stage_allclear");
    }

    public override void OnHideAfter()
    {
        base.OnHideAfter();

        OnUIHide?.Invoke();
        OnUIHide = null;

    }
}
