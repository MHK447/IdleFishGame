using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public List<RecordCount> Recordvalue { get; private set; } = new List<RecordCount>();

    public List<RecordCount> Recordcount { get; private set; } = new List<RecordCount>();
    private void SaveData_RecordCount(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Recordvalue Array 저장
        Offset<BanpoFri.Data.RecordCount>[] recordvalue_Array = null;
        VectorOffset recordvalue_Vector = default;

        if(Recordvalue.Count > 0){
            recordvalue_Array = new Offset<BanpoFri.Data.RecordCount>[Recordvalue.Count];
            int index = 0;
            foreach(var pair in Recordvalue){
                var item = pair;
                recordvalue_Array[index++] = BanpoFri.Data.RecordCount.CreateRecordCount(
                    builder,
                    builder.CreateString(item.Idx),
                    item.Count
                );
            }
            recordvalue_Vector = BanpoFri.Data.UserData.CreateRecordvalueVector(builder, recordvalue_Array);
        }

        // Recordcount Array 저장
        Offset<BanpoFri.Data.RecordCount>[] recordcount_Array = null;
        VectorOffset recordcount_Vector = default;

        if(Recordcount.Count > 0){
            recordcount_Array = new Offset<BanpoFri.Data.RecordCount>[Recordcount.Count];
            int index = 0;
            foreach(var pair in Recordcount){
                var item = pair;
                recordcount_Array[index++] = BanpoFri.Data.RecordCount.CreateRecordCount(
                    builder,
                    builder.CreateString(item.Idx),
                    item.Count
                );
            }
            recordcount_Vector = BanpoFri.Data.UserData.CreateRecordcountVector(builder, recordcount_Array);
        }



        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddRecordvalue(builder, recordvalue_Vector);
            BanpoFri.Data.UserData.AddRecordcount(builder, recordcount_Vector);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_RecordCount()
    {
        // 로드 함수 내용

        // Recordvalue 로드
        Recordvalue.Clear();
        int Recordvalue_length = flatBufferUserData.RecordvalueLength;
        for (int i = 0; i < Recordvalue_length; i++)
        {
            var Recordvalue_item = flatBufferUserData.Recordvalue(i);
            if (Recordvalue_item.HasValue)
            {
                var recordcount = new RecordCount
                {
                    Idx = Recordvalue_item.Value.Idx,
                    Count = Recordvalue_item.Value.Count
                };
                Recordvalue.Add(recordcount);
            }
        }

        // Recordcount 로드
        Recordcount.Clear();
        int Recordcount_length = flatBufferUserData.RecordcountLength;
        for (int i = 0; i < Recordcount_length; i++)
        {
            var Recordcount_item = flatBufferUserData.Recordcount(i);
            if (Recordcount_item.HasValue)
            {
                var recordcount = new RecordCount
                {
                    Idx = Recordcount_item.Value.Idx,
                    Count = Recordcount_item.Value.Count
                };
                Recordcount.Add(recordcount);
            }
        }
    }

}

public class RecordCount
{
    public string Idx { get; set; } = "";
    public int Count { get; set; } = 0;

}
