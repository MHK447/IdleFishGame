using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using global::Google.FlatBuffers;
using BanpoFri.Data;
using System.Linq;

public partial class UserDataSystem
{
    private readonly string dataFileName = "Master.dat";
    private readonly string backupTimeKey = "LAST_BACKUP_TIME";
    private readonly string backupFormat = "savebackup";
    private readonly int backupPeriod = 7200;

    private BanpoFri.Data.UserData flatBufferUserData;
    private float saveWaitStandardTime = 30f;
    private float deltaTime = 0f;
    private bool saving = false;

    private bool isSafeData = true;

    public void Update()
    {
        if (saving)
        {
            if (deltaTime > saveWaitStandardTime)
            {
                saving = false;
                SaveFile();
                deltaTime = 0f;
                return;
            }
            deltaTime += Time.deltaTime;
        }
    }




    public string GetSaveFilePath()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return $"{Application.persistentDataPath}/{dataFileName}";
        }
        else
        {
            return $"{Application.dataPath}/{dataFileName}";
        }
    }

    public string GetBackUpSaveFilePath(string backupformat)
    {
        return $"{GetSaveFilePath()}.{backupformat}";
    }

    public void InitDataState()
    {
        DataState = DataState.None;
    }

    public void Load()
    {
        var filePath = GameRoot.Instance.UserData.GetBackUpSaveFilePath("backup");
        if (File.Exists(filePath))
        {
            mainData = new UserDataMain();
            eventData = new UserDataEvent();
            var data = File.ReadAllBytes(filePath);
            ByteBuffer bb = new ByteBuffer(data);
            flatBufferUserData = BanpoFri.Data.UserData.GetRootAsUserData(bb);
            ConnectReadOnlyDatas();
            File.Delete(filePath);
            Save(true);
            return;
        }

#if UNITY_EDITOR
        var key = "CustomSaveDataKey";
        if (UnityEditor.EditorPrefs.HasKey(key))
        {
            filePath = $"{Application.dataPath}/{UnityEditor.EditorPrefs.GetString(key)}.dat";
        }
        else
#endif
        {
            filePath = GetSaveFilePath();
        }

        if (File.Exists(filePath))
        {
            var data = File.ReadAllBytes(filePath);
            ByteBuffer bb = new ByteBuffer(data);
            try
            {
                flatBufferUserData = BanpoFri.Data.UserData.GetRootAsUserData(bb);
                ConnectReadOnlyDatas();
            }
            catch (Exception ex)
            {
                TpLog.LogError("Data Error!!" + ex.Message);
                isSafeData = false;
            }
            finally
            {
                if (!isSafeData)
                {
                    LoadBackupFile();
                }
            }
        }
        else
        {
            ChangeDataMode(DataState.Main);
        }
    }

    private void LoadBackupFile()
    {
        var filePath = GetBackUpSaveFilePath(backupFormat);

        if (File.Exists(filePath))
        {
            var data = File.ReadAllBytes(filePath);

            ByteBuffer bb = new ByteBuffer(data);
            flatBufferUserData = BanpoFri.Data.UserData.GetRootAsUserData(bb);
            bool isSuccess = true;
            try
            {
                ConnectReadOnlyDatas();
            }
            catch (Exception ex)
            {
                TpLog.LogError("Data Error!! Backup : " + ex.Message);
                isSuccess = false;
            }

            if (isSuccess)
            {
                isSafeData = true;
                Save(true);
            }
        }
    }

    public void Save(bool Immediately = false)
    {
        if (Immediately)
        {
            SaveFile();
            return;
        }
        if (saving)
        {
            //deltaTime = 0f;
            return;
        }

        saving = true;
        //deltaTime = 0f;
    }

    // @저장 함수 콜백
    Action cb_SaveAddDatas = null;
    void SetSaveDatas(FlatBufferBuilder builder){
        /* 아래 @주석 위치를 찾아서 함수가 자동 추가됩니다 SaveFile 함수에서 SetSaveDatas를 호출해주세요 */
        // @자동 저장 데이터 함수들
        SaveData_UpgradeData(builder);
        SaveData_AquariumData(builder);
        SaveData_RecordCount(builder);
        SaveData_OptionData(builder);
    }    
    private void SaveFile()
    {
        if (!isSafeData)
        {
            TpLog.LogError("Data Error!! Can not save file");
            return;
        }


        //CompleteHistory.Clear();
        var builder = new FlatBufferBuilder(1);
        int dataIdx = 0;
        var buyInappIds = builder.CreateString(string.Join(";", BuyInappIds));

        //option
        var option = BanpoFri.Data.OptionData.CreateOptionData(builder, builder.CreateString(Language.ToString()), Bgm, Effect, SlowGraphic, Vib, SubscribeOrder, AutoFelling);

        Offset<BanpoFri.Data.RecordCount>[] recordCount = null;
        if (RecordCount.Count > 0)
        {
            recordCount = new Offset<BanpoFri.Data.RecordCount>[RecordCount.Count];
            dataIdx = 0;
            foreach (var rc in RecordCount)
            {
                recordCount[dataIdx++] = BanpoFri.Data.RecordCount.CreateRecordCount(builder, builder.CreateString(rc.Key), rc.Value);
            }
        }
        VectorOffset recordCountVec = default(VectorOffset);
        if (recordCount != null)
            recordCountVec = BanpoFri.Data.UserData.CreateRecordcountVector(builder, recordCount);

        Offset<BanpoFri.Data.RecordCount>[] recordvalue = null;
       

        //add userdata
        SetSaveDatas(builder);
       
      

        // tutorial
        StringOffset[] tutorialArray = null;
        if (Tutorial.Count > 0) {
            tutorialArray = new StringOffset[Tutorial.Count];
            for (int i = 0; i < Tutorial.Count; i++) {
                tutorialArray[i] = builder.CreateString(Tutorial[i]);
            }
        }
        VectorOffset tutorialVec = default(VectorOffset);
        if (tutorialArray != null)
            tutorialVec = BanpoFri.Data.UserData.CreateTutorialVector(builder, tutorialArray);


        // @add userdata

        BanpoFri.Data.UserData.StartUserData(builder);
        // @저장 함수 콜백 호출
        cb_SaveAddDatas?.Invoke();
        cb_SaveAddDatas = null;
        BanpoFri.Data.UserData.AddCash(builder, Cash.Value);
        BanpoFri.Data.UserData.AddBuyinappids(builder, buyInappIds);
        BanpoFri.Data.UserData.AddLastlogintime(builder, mainData.LastLoginTime.Ticks);
        BanpoFri.Data.UserData.AddOptiondata(builder, option);
        BanpoFri.Data.UserData.AddRecordcount(builder, recordCountVec);
        BanpoFri.Data.UserData.AddTutorial(builder, tutorialVec);
        BanpoFri.Data.UserData.AddFishingautoproperty(builder, Fishingautoproperty.Value);
        var orc = BanpoFri.Data.UserData.EndUserData(builder);
        builder.Finish(orc.Value);

        var dataBuf = builder.DataBuffer;
        flatBufferUserData = BanpoFri.Data.UserData.GetRootAsUserData(dataBuf);

        var filePath = GetSaveFilePath();
        using (var ms = new MemoryStream(flatBufferUserData.ByteBuffer.ToFullArray(), dataBuf.Position, builder.Offset))
        {
            var binary = Convert.ToBase64String(ms.ToArray());

            if (binary.Length > 0)
            {
                try { File.WriteAllBytes(filePath, ms.ToArray()); }
                catch (Exception ex) { Debug.Log("Failed Save File  = " + ex.Message); }
            }

            //    if (binary.Length > 0 && GameRoot.Instance.ExceptionManager.ErrorCount == 0)
            //    {
            //        var bBackup = false;
            //        if (PlayerPrefs.HasKey(backupTimeKey))
            //        {
            //            long backupTimeTicks = 0;
            //            if (long.TryParse(PlayerPrefs.GetString(backupTimeKey), out backupTimeTicks))
            //            {
            //                var curTime = TimeSystem.GetCurTime();
            //                var backupTime = new System.DateTime(backupTimeTicks);

            //                if ((int)curTime.Subtract(backupTime).TotalSeconds >= backupPeriod)
            //                    bBackup = true;
            //            }
            //        }
            //        else bBackup = true;

            //        if (bBackup)
            //        {
            //            var backuppath = GetBackUpSaveFilePath(backupFormat);
            //            try { File.WriteAllBytes(backuppath, ms.ToArray()); }
            //            catch (Exception ex) { Debug.Log("Failed Save File  = " + ex.Message); }
            //        }
            //    }
            //    else if (GameRoot.Instance.ExceptionManager.ErrorCount > 0)
            //    {
            //        Debug.Log("Error Log is Larger than 0");
            //    }

            //}
        }
    }
}
