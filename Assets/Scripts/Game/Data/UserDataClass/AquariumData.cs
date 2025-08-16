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
        // Aquariumdata.Fishidxs 처리 GenerateItemSaveCode Array
        int[] aquariumdata_fishidxs_Array = null;
        VectorOffset aquariumdata_fishidxs_Vector = default;

        if(Aquariumdata.Fishidxs.Count > 0){
            aquariumdata_fishidxs_Array = new int[Aquariumdata.Fishidxs.Count];
            int aquariumdata_fishidxs_idx = 0;
            foreach(int aquariumdata_fishidxs_val in Aquariumdata.Fishidxs){
                aquariumdata_fishidxs_Array[aquariumdata_fishidxs_idx++] = aquariumdata_fishidxs_val;
            }
            aquariumdata_fishidxs_Vector = BanpoFri.Data.AquariumData.CreateFishidxsVector(builder, aquariumdata_fishidxs_Array);
        }

        // Aquariumdata 최종 생성 및 추가
        var aquariumdata_Offset = BanpoFri.Data.AquariumData.CreateAquariumData(
            builder,
            aquariumdata_fishidxs_Vector
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

            // Fishidxs 리스트 로드
            Aquariumdata.Fishidxs.Clear();
            for (int j = 0; j < fb_Aquariumdata.Value.FishidxsLength; j++)
            {
                int fishidxs_val = fb_Aquariumdata.Value.Fishidxs(j);
                Aquariumdata.Fishidxs.Add(fishidxs_val);
            }
        }
    }

}

public class AquariumData
{
    public List<int> Fishidxs = new List<int>();

}
