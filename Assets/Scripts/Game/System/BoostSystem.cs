using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UniRx;

public class BoostSystem
{

    public IReactiveProperty<bool> IsBoostOnProperty = new ReactiveProperty<bool>(false);

    public int boost_time = 0;


    public int BoostValue = 2;


    public void Create()
    {
        boost_time = Tables.Instance.GetTable<Define>().GetData("boost_time").value;

         IsBoostOnProperty.Value = GameRoot.Instance.UserData.CurMode.BoostTime.Value > 0;
    }


    public void AddBoosTime()
    {
        IsBoostOnProperty.Value = true;

        GameRoot.Instance.UserData.CurMode.BoostTime.Value = boost_time;

    }

    public void BoostOff()
    {
        GameRoot.Instance.UserData.CurMode.BoostTime.Value = 0;
        IsBoostOnProperty.Value = false;
    }


    public void UpdateOneSecond()
    {
        if (IsBoostOnProperty.Value)
        {
            GameRoot.Instance.UserData.CurMode.BoostTime.Value -= 1;

            if (GameRoot.Instance.UserData.CurMode.BoostTime.Value <= 0)
            {
                BoostOff();
            }
        }
    }

}
