using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using BanpoFri;
using BanpoFri.Data;

public enum DataState
{
    None,
    Main,
    Event,
    Travel,
}
public partial class UserDataSystem
{
    public bool Bgm = true;
    public bool Effect = true;
    public bool Vib = true;
    public bool SlowGraphic = false;
    public long UUID { get; private set; } = 0;

    public Config.Language Language = Config.Language.ko;
    public IReactiveProperty<int> Garnet { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> UpgradeStone { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Cash { get; private set; } = new ReactiveProperty<int>(0);
    public int ABTestValue { get; set; } = -1;

    public IReactiveCollection<string> BuyInappIds { get; private set; } = new ReactiveCollection<string>();
    public Dictionary<string, int> RecordCount { get; private set; } = new Dictionary<string, int>();
    public Dictionary<string, int> RecordValue { get; private set; } = new Dictionary<string, int>();
    public System.DateTime FirstStartTime = default(System.DateTime);

    public IReactiveCollection<string> GameNotifications { get; private set; } = new ReactiveCollection<string>();
    public IReactiveCollection<int> EquipCostumes { get; set; } = new ReactiveCollection<int>();

    public Queue<int> RandomSeeds = new Queue<int>();

    public int PiggyCash { get; set; } = 0;
    public bool AutoFelling { get; set; } = false;
    public bool SubscribeOrder { get; set; } = true;

    //public System.DateTime LastScheduleViewTime { get; set; } = default(System.DateTime);


    public IUserDataMode CurMode { get; private set; }
    public UserDataMain mainData { get; private set; } = new UserDataMain();


    private UserDataEvent eventData = new UserDataEvent();

    //public ReactiveCollection<SellerItemData> SellerItemData { get; set; } = new ReactiveCollection<SellerItemData>();
    public System.DateTime SellerTime { get; set; }

    public DataState DataState { get; private set; } = DataState.None;
    public bool IsMainState { get { return DataState == DataState.Main; } }
    public int WatchCount = 0;
    public int WatchInterval = 0;
    public BigInteger HeartBeatCount = 1;
    //public int StarRewardOrder = 1;

    public GameType LastMode = GameType.Main;

    public IReactiveProperty<BigInteger> HUDMoney = new ReactiveProperty<BigInteger>(0);
    public IReactiveProperty<int> HUDGarnet = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> HUDCash = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> HUDAdsTicket = new ReactiveProperty<int>(0);
    public IReactiveProperty<double> HUDMineral = new ReactiveProperty<double>(0);
    public IReactiveProperty<double> HUDArtifactStone = new ReactiveProperty<double>(0);

    // @변수 자동 등록 위치
    public IReactiveProperty<bool> Fishingautoproperty { get; set; } = new ReactiveProperty<bool>(false);
    public DateTime Dayinitialtime { get; set; }
    public int Energycreatefood { get; set; } = 0;
    public IReactiveProperty<int> Starcoinvalue { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Starvalue { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Energycoin { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Stageenergycount { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Nextstagecount { get; private set; } = new ReactiveProperty<int>(0);
    public List<int> Pobtest = new List<int>();
    public List<int> Ordertest = new List<int>();
    public int Testmoney { get; set; } = 0;
    public List<string> Tutorial = new List<string>();
    public IReactiveProperty<double> Money { get; private set; } = new ReactiveProperty<double>(0.0f);
    public int Abtestvalue { get; set; } = 0;
    public long Uuid { get; set; } = 0;
    public long Gamestarttime { get; set; } = 0;
    public long Lastlogintime { get; set; } = 0;
    public string Buyinappids { get; set; } = "";


    void SetLoadDatas()
    {
        /* 아래 @주석 위치를 찾아서 함수가 자동 추가됩니다 ConnectReadOnlyDatas 함수에서 SetLoadDatas를 호출해주세요 */
        // @자동 로드 데이터 함수들
        LoadData_RecordCount();
        LoadData_OptionData();
    }
    void ConnectReadOnlyDatas()
    {
        GameNotifications.Clear();
        BuyInappIds.Clear();






        FirstStartTime = new System.DateTime(flatBufferUserData.Gamestarttime);

        BuyInappIds.Clear();
        if (!string.IsNullOrEmpty(flatBufferUserData.Buyinappids))
        {
            var splitArr = flatBufferUserData.Buyinappids.Split(';');
            foreach (var split in splitArr)
            {
                BuyInappIds.Add(split);
            }
        }


        if (flatBufferUserData.Uuid != 0)
            UUID = flatBufferUserData.Uuid;


        RecordCount.Clear();
        for (int i = 0; i < flatBufferUserData.RecordcountLength; ++i)
        {
            var data = flatBufferUserData.Recordcount(i);

            RecordCount.Add(data.Value.Idx, data.Value.Count);
        }


        RecordValue.Clear();
        for (int i = 0; i < flatBufferUserData.RecordvalueLength; ++i)
        {
            var data = flatBufferUserData.Recordvalue(i);
            RecordValue.Add(data.Value.Idx, data.Value.Count);
        }



        mainData.LastLoginTime = new System.DateTime(flatBufferUserData.Lastlogintime);

        HUDGarnet.Value = Garnet.Value = flatBufferUserData.Cash;

        // @로드 함수 호출
        SetLoadDatas();

        // @변수 자동 데이터 추가
        Fishingautoproperty.Value = flatBufferUserData.Fishingautoproperty;
        Cash.Value = flatBufferUserData.Cash;



        ChangeDataMode(LastMode == GameType.Event ? DataState.Event : DataState.Main);

        Language = (Config.Language)System.Enum.Parse(typeof(Config.Language), flatBufferUserData.Optiondata.Value.Language);
        Bgm = flatBufferUserData.Optiondata.Value.Bgm;
        Effect = flatBufferUserData.Optiondata.Value.Effect;
        SlowGraphic = flatBufferUserData.Optiondata.Value.Slowgraphic;
        Vib = flatBufferUserData.Optiondata.Value.Vibration;
        AutoFelling = flatBufferUserData.Optiondata.Value.Autofelling;
        SubscribeOrder = flatBufferUserData.Optiondata.Value.Subscribeorder;


        // var curStage = flatBufferUserData.stage;
        // mainData.StageData.SetStageIdx(curStage);

        //Debug.Log("stagedata:" + curStage);
    }

    public void ChangeDataMode(DataState state)
    {
        if (state == DataState)
            return;

        TpLog.Log($"ChangeDataMode:{state.ToString()}");
        switch (state)
        {
            case DataState.Main:
                CurMode = mainData;
                break;
            case DataState.Event:
                CurMode = eventData;
                break;
        }

        DataState = state;
    }


    public UserDataEvent CurEventData { get { return eventData; } }

    private void SnycCollectionToDB<T, U>(IList<T> db, IEnumerable<U> collector) where T : class
    {
        db.Clear();
        foreach (var iter in collector)
        {
            db.Add(iter as T);
        }
    }

    private void SnycCollectionToClient<T, U>(IList<T> db, IEnumerable<U> collector)
    where T : class, IReadOnlyData
    where U : class, IReadOnlyData
    {
        db.Clear();
        foreach (var iter in collector)
        {
            db.Add(iter.Clone() as T);
        }
    }

    public void SetRecordValue(Config.RecordKeys key, int value, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordValueText(key, objs);
        if (RecordValue.ContainsKey(strKey))
        {
            RecordValue[strKey] = value;
        }
        else
        {
            RecordValue.Add(strKey, value);
        }

        Save();
    }

    //public int GetABUser()
    //{
    //    if (GetRecordValue(Config.RecordKeys.AttTestUser) < 0)
    //        return 1;
    //    else
    //        return GetRecordValue(Config.RecordKeys.AttTestUser);
    //}

    public int GetRecordValue(Config.RecordKeys key, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordValueText(key, objs);
        if (RecordValue.ContainsKey(strKey))
        {
            return RecordValue[strKey];
        }
        else
        {
            return -1;
        }
    }

    public void ResetContainKeyRecordCount(Config.RecordCountKeys idx)
    {
        foreach (var key in RecordCount.Keys)
        {
            if (key.Contains(idx.ToString()))
            {
                RecordCount[key] = 0;
            }
        }
    }

    public void ResetRecordCount(Config.RecordCountKeys idx, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);
        if (RecordCount.ContainsKey(strKey))
            RecordCount[strKey] = 0;
    }


    public void ResetRecordCount(Config.RecordCountKeys idx, int resetvalue = 0, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);
        if (RecordCount.ContainsKey(strKey))
            RecordCount[strKey] = resetvalue;
    }



    public void AddRecordCount(Config.RecordCountKeys idx, int count, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);
        if (RecordCount.ContainsKey(strKey))
            RecordCount[strKey] += count;
        else
            RecordCount.Add(strKey, count);
    }

    public int GetRecordCount(Config.RecordCountKeys idx, params object[] objs)
    {
        var strKey = ProjectUtility.GetRecordCountText(idx, objs);

        if (RecordCount.ContainsKey(strKey))
        {
            return RecordCount[strKey];
        }
        else
        {
            return 0;
        }
    }

    public void SyncHUDCurrency(int currencyID = -1)
    {
        if (currencyID < 0)
        {
            HUDGarnet.Value = Garnet.Value;
            HUDCash.Value = Cash.Value;
        }
    }

    public void SetReward(int rewardType, int rewardIdx, System.Numerics.BigInteger rewardCnt, bool hudRefresh = true)
    {
        switch ((Config.RewardType)rewardType)
        {
            case Config.RewardType.Currency:
                {
                    switch (rewardIdx)
                    {

                        case (int)Config.CurrencyID.Cash:
                            {
                                Cash.Value += (int)rewardCnt;
                            }
                            break;
                    }
                    GameRoot.Instance.UserData.Save();
                }
                break;
        }

        if (hudRefresh)
        {
            SyncHUDCurrency(rewardIdx);
        }
    }

    public void AddPiggyCash()
    {
        var value = 0;//Tables.Instance.GetTable<Define>().GetData("piggybank_get_count").value;
        var max_value = 0;//Tables.Instance.GetTable<Define>().GetData("piggybank_full_count").value;
        if (GameRoot.Instance.UserData.PiggyCash >= max_value)
        {
            return;
        }

        if (GameRoot.Instance.UserData.PiggyCash + value >= max_value)
            GameRoot.Instance.UserData.PiggyCash = max_value;
        else
            GameRoot.Instance.UserData.PiggyCash += value;
    }



    public void UnEquipCostume(int idx)
    {
        if (EquipCostumes.Contains(idx))
        {
            EquipCostumes.Remove(idx);
        }

        Save();
    }


    public void RemoveEventData()
    {

        //userEventData = new EventData();
        eventData = new UserDataEvent();
        Save();
    }


}
