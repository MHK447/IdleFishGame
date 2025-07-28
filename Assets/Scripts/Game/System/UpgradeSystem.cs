using BanpoFri;
using UnityEngine;

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
            for (int i = 0; i < (int)UpgradeType.PriceMulti; i++)
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
}
