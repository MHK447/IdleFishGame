using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using System.Numerics;
using System;

[UIPath("UI/InGame/NewFacilityUI", false)]
public class NewFacilityUI : InGameFloatingUI
{
    [SerializeField]
    private Text MoneyText;

    [SerializeField]
    private Image RewardImg;

    [SerializeField]
    private Image SliderImg;


    public void SliderValue(System.Numerics.BigInteger rewardcount , System.Numerics.BigInteger goalcount)
    {
        MoneyText.text = Utility.CalculateMoneyToString(goalcount);
        double logReward = BigInteger.Log(rewardcount);
        double logGoal = BigInteger.Log(goalcount);

        // 두 로그의 차를 이용해 비율을 계산합니다.
        double ratio = Math.Exp(logReward - logGoal);

        // 계산된 비율이 1을 넘지 않도록 0~1 범위로 보정합니다.
        SliderImg.fillAmount = Mathf.Clamp01((float)ratio);
    }

}
