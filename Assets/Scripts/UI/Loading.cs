using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using UnityEngine.AddressableAssets;

public class Loading : UIBase
{
    [SerializeField]
    private CanvasGroup group;
    [SerializeField]
    private GameObject FullScreen;
    [SerializeField]
    private GameObject Spinner;
    [SerializeField]
    private float hideTime = 3f;
    [SerializeField]
    private float hideHoldTime = 4f;

    [SerializeField]
    private Image BeforeShopImg;
    [SerializeField]
    private Image AfterShopImg;
    [SerializeField]
    private Text BeforeShopName;
    [SerializeField]
    private Text AfterShopName;

    [SerializeField]
    private Text TipText;

    private float deletaTime = 0f;
    private bool hideAni = false;
    private GameObject instSprite = null;
    private System.Action HideCallback;

    protected override void Awake()
    {
        base.Awake();
    }

    public void RefreshText()
    {
    }

    public  void Show(bool worldTranslate)
    {
        ProjectUtility.SetActiveCheck(gameObject, true);
        ProjectUtility.SetActiveCheck(FullScreen, worldTranslate);
        ProjectUtility.SetActiveCheck(Spinner, !worldTranslate);
        group.alpha = 1f;

    }

    public void LockScreen()
    {
        ProjectUtility.SetActiveCheck(gameObject, true);
        ProjectUtility.SetActiveCheck(FullScreen, true);
        ProjectUtility.SetActiveCheck(Spinner, false);
        group.alpha = 0.001f;
    }

    public void ShowTip(string str)
    {
        if (TipText == null) return;

        ProjectUtility.SetActiveCheck(TipText.gameObject, true);


        TipText.text = Tables.Instance.GetTable<Localize>().GetString(str);
    }

    public void SetStageImage()
    {
       
        // var PrevTd = Tables.Instance.GetTable<StageSet>().GetData(curStage - 1);
        // var Td = Tables.Instance.GetTable<StageSet>().GetData(curStage);

        // BeforeShopImg.sprite = Config.Instance.GetRenovateImg(PrevTd != null ? PrevTd.stage_icon : Td.stage_icon);
        // AfterShopImg.sprite = Config.Instance.GetRenovateImg(Td.stage_icon);
        // if (BeforeShopName != null)
        //     BeforeShopName.text = Tables.Instance.GetTable<Localize>().GetString(PrevTd != null ? PrevTd.stage_name : Td.stage_name);
        // if (AfterShopName != null)
        //     AfterShopName.text = Tables.Instance.GetTable<Localize>().GetString(Td.stage_name);
    }

    public void SetRegionImage()
    {
        // var curStage = Tables.Instance.GetTable<StageSet>().GetData(GameRoot.Instance.UserData.CurMode.StageData.StageIdx);
        // var prevStage = Tables.Instance.GetTable<StageSet>().GetData(GameRoot.Instance.UserData.CurMode.StageData.StageIdx - 1);
        // var CurRegion = Tables.Instance.GetTable<Region>().GetData(curStage.region);
        // var PrevRegion = Tables.Instance.GetTable<Region>().GetData(prevStage != null ? prevStage.region : curStage.region);

        // BeforeShopImg.sprite = Config.Instance.GetRenovateImg(PrevRegion.region_icon);
        // AfterShopImg.sprite = Config.Instance.GetRenovateImg(CurRegion.region_icon);
        // if (BeforeShopName != null)
        //     BeforeShopName.text = Tables.Instance.GetTable<Localize>().GetString(PrevRegion.region_name);
        // if (AfterShopName != null)
        //     AfterShopName.text = Tables.Instance.GetTable<Localize>().GetString(CurRegion.region_name);
    }

    public void Hide(bool Immediately = true, System.Action action = null)
    {
        HideCallback = action;

        if (Immediately)
        {
            ProjectUtility.SetActiveCheck(gameObject, false);
            HideCallback?.Invoke();
        }
        else
            hideAni = true;

        if(!this.gameObject.activeSelf)
        {
            HideCallback?.Invoke();
            hideAni = false;
        }

        deletaTime = 0f;
    }

    private void Update()
    {
        if(hideAni)
        {
            if(deletaTime > hideHoldTime)
            {
                if(deletaTime >= hideTime)
                {
                    ProjectUtility.SetActiveCheck(gameObject, false);
                    HideCallback?.Invoke();
                    hideAni = false;
                }
                group.alpha = 1f - (deletaTime / hideTime);
            }
            
            deletaTime += Time.deltaTime;
        }
    }
}
