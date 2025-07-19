using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;


public class TutorialEntitySetReward : TutorialEntity
{
    [SerializeField]
    private Config.RewardType RewardType;

    [SerializeField]
    private int RewardIdx = 0;


    [SerializeField]
    private int RewardCount = 0;

    public override void StartEntity()
    {
        base.StartEntity();

        switch(RewardType)
        {
        
        }


        Done();
    }
   
}
