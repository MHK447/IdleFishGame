using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public List<UpgradeData> Upgradedata { get; private set; } = new List<UpgradeData>();



    private void SaveData_UpgradeData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Upgradedata Array 저장
        Offset<BanpoFri.Data.UpgradeData>[] upgradedata_Array = null;
        VectorOffset upgradedata_Vector = default;

        if(Upgradedata.Count > 0){
            upgradedata_Array = new Offset<BanpoFri.Data.UpgradeData>[Upgradedata.Count];
            int index = 0;
            foreach(var pair in Upgradedata){
                var item = pair;
                upgradedata_Array[index++] = BanpoFri.Data.UpgradeData.CreateUpgradeData(
                    builder,
                    item.Upgradeidx,
                    item.Upgradelevel
                );
            }
            upgradedata_Vector = BanpoFri.Data.UserData.CreateUpgradedataVector(builder, upgradedata_Array);
        }



        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddUpgradedata(builder, upgradedata_Vector);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_UpgradeData()
    {
        // 로드 함수 내용

        // Upgradedata 로드
        Upgradedata.Clear();
        int Upgradedata_length = flatBufferUserData.UpgradedataLength;
        for (int i = 0; i < Upgradedata_length; i++)
        {
            var Upgradedata_item = flatBufferUserData.Upgradedata(i);
            if (Upgradedata_item.HasValue)
            {
                var upgradedata = new UpgradeData
                {
                    Upgradeidx = Upgradedata_item.Value.Upgradeidx,
                    Upgradelevel = Upgradedata_item.Value.Upgradelevel
                };
                Upgradedata.Add(upgradedata);
            }
        }
    }

}

public class UpgradeData
{
    public int Upgradeidx { get; set; } = 0;
    public int Upgradelevel { get; set; } = 0;

}
