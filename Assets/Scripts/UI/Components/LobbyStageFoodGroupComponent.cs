using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class LobbyStageFoodGroupComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI StagePercentText;

    [SerializeField]
    private Image ProgressStoreImage;


    [SerializeField]
    private Image StoreBgImage;

    [SerializeField]
    private Slider SliderValue;


    private int FoodMergeGroupIdx = 0;

    private int ClearGoalCount = 0;


    private CompositeDisposable disposables = new CompositeDisposable();


    public void Set(int foodmergegroupidx)
    {
        FoodMergeGroupIdx = foodmergegroupidx;
    }

    public void SetStageClearCheck(int curcount)
    {
        SliderValue.value = (float)curcount / (float)ClearGoalCount;

        var percentvalue = ProgressStoreImage.fillAmount * 100;

        StagePercentText.text = $"{percentvalue.ToString("F0")}%";
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

}
