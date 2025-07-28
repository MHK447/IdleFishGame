using BanpoFri;
using UnityEngine;
using System.Numerics;
using System;

public class UpgradeSystem
{

    public enum UpgradeType
    {
        FisihngSpeeed,
        WaterDepth,
        PriceMulti,
    }

    public void Create()
    {
        if (GameRoot.Instance.UserData.Upgradedata.Count == 0)
        {
            for (int i = 0; i < (int)UpgradeType.PriceMulti + 1; i++)
            {
                GameRoot.Instance.UserData.Upgradedata.Add(new UpgradeData() { Upgradeidx = i, Upgradelevel = 1 });
            }
        }

    }




    public float GetUpgradeValue(UpgradeType type)
    {

        float value = 0f;
        var td = Tables.Instance.GetTable<UpgradeInfo>().GetData((int)type);

        if (td != null)
        {
            var level = GameRoot.Instance.UserData.Upgradedata[(int)type].Upgradelevel;


            value = td.start_value * (td.level_up_value * level);

        }


        return value;

    }


    public BigInteger GetUpgradeCost(int level, int baseCost, float power)
    {
        if (level <= 0) return new BigInteger(baseCost);

        double costMultiplier = Math.Pow(level, power);
        BigInteger finalCost = baseCost * new BigInteger(costMultiplier);
        return finalCost;
    }
}
