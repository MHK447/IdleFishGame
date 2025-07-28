using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public AquariumData Aquariumdata { get; private set; } = new AquariumData();
    private void SaveData_AquariumData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Aquariumdata 단일 저장
        // Aquariumdata 최종 생성 및 추가
        var aquariumdata_Offset = BanpoFri.Data.AquariumData.CreateAquariumData(
            builder,
            Aquariumdata.Fishidx
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddAquariumdata(builder, aquariumdata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_AquariumData()
    {
        // 로드 함수 내용

        // Aquariumdata 로드
        var fb_Aquariumdata = flatBufferUserData.Aquariumdata;
        if (fb_Aquariumdata.HasValue)
        {
            Aquariumdata.Fishidx = fb_Aquariumdata.Value.Fishidx;
        }
    }

}

public class AquariumData
{
    public int Fishidx { get; set; } = 0;

}
