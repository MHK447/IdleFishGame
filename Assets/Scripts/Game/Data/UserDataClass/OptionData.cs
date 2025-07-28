using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public OptionData Optiondata { get; private set; } = new OptionData();
    private void SaveData_OptionData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Optiondata 단일 저장
        // Optiondata 최종 생성 및 추가
        var optiondata_Offset = BanpoFri.Data.OptionData.CreateOptionData(
            builder,
            builder.CreateString(Optiondata.Language),
            Optiondata.Bgm,
            Optiondata.Effect,
            Optiondata.Slowgraphic,
            Optiondata.Vibration,
            Optiondata.Subscribeorder,
            Optiondata.Autofelling
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddOptiondata(builder, optiondata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_OptionData()
    {
        // 로드 함수 내용

        // Optiondata 로드
        var fb_Optiondata = flatBufferUserData.Optiondata;
        if (fb_Optiondata.HasValue)
        {
            Optiondata.Language = fb_Optiondata.Value.Language;
            Optiondata.Bgm = fb_Optiondata.Value.Bgm;
            Optiondata.Effect = fb_Optiondata.Value.Effect;
            Optiondata.Slowgraphic = fb_Optiondata.Value.Slowgraphic;
            Optiondata.Vibration = fb_Optiondata.Value.Vibration;
            Optiondata.Subscribeorder = fb_Optiondata.Value.Subscribeorder;
            Optiondata.Autofelling = fb_Optiondata.Value.Autofelling;
        }
    }

}

public class OptionData
{
    public string Language { get; set; } = "";
    public bool Bgm { get; set; } = false;
    public bool Effect { get; set; } = false;
    public bool Slowgraphic { get; set; } = false;
    public bool Vibration { get; set; } = false;
    public bool Subscribeorder { get; set; } = false;
    public bool Autofelling { get; set; } = false;

}
