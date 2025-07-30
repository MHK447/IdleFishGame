using System.Collections.Generic;
using System.Linq;
using BanpoFri;
using UnityEngine;
using UniRx;
using System;
public enum InGameType
{
    Main,
    Event,
}
[System.Serializable]
public enum GameType
{
    Main,
    Event,
    Travel,

    None = 99,
}

public class InGameSystem
{
    public SceneSystem SceneSystem { get; private set; } = new SceneSystem();
    public InGameMode CurInGame { get; private set; } = null;

    private bool firstInit = false;
    private bool IsActiveEventTuto = false;
    private bool IsTitle = false;

    private System.Action NextStageAction = null;
    private System.Action NextStageCloseAction = null;

    public bool nextStage = false;

    public System.Action NextActionClear = null;

    public IReactiveProperty<int> LevelProperty = new ReactiveProperty<int>();
    public IReactiveProperty<int> DeadCount = new ReactiveProperty<int>();


    public int TicketEnemyIdx = 10001;

    public int CounterIdx = 1000;

    CompositeDisposable disposables = new CompositeDisposable();


    public float casher_move_speed = 0f;
    public float carry_sleep_time = 0f;
    public int player_start_carry_count = 0;
    public int carry_casher_count = 0;
    public int max_offline_time = 0;
    public int offline_value_time = 0;

    public int stage_energy_consume = 0;

    public int offline_reward_multiple = 0;

    public float default_fishing_time = 0;


    public void Create()
    {
        stage_energy_consume = Tables.Instance.GetTable<Define>().GetData("stage_energy_consume").value;
    }

    public T GetInGame<T>() where T : InGameMode
    {
        return CurInGame as T;
    }

    public void RegisteInGame(InGameMode mode)
    {
        CurInGame = mode;
    }
    public void ChangeMode(InGameType type)
    {
        // Tables.Instance.GetTable<FacilityUpgrade>().CalculateUpgradeTable(GameRoot.Instance.UserData.CurMode.StageData.StageIdx);
        System.GC.Collect();
        StartGame(type, () =>
        {
            firstInit = false;
        });
    }
    private void StartGame(InGameType type, System.Action loadCallback = null)
    {
        GameRoot.Instance.Loading.Show(true);
        SceneSystem.ChangeScene(type, loadCallback);
    }


    public void NextGameStage(bool Init = false)
    {

        // 1. 현재 인게임 명시적 언로드 및 리소스 정리
        if (GameRoot.Instance.InGameSystem.CurInGame != null)
        {
            try
            {
                // 인게임 언로드
                GameRoot.Instance.InGameSystem.CurInGame.UnLoad();

                // GC 실행 요청
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error during game unloading: " + e.Message);
            }
        }

        // 2. 스테이지 데이터 업데이트
        var saveTime = TimeSystem.GetCurTime().Ticks;
        GameRoot.Instance.UserData.CurMode.SelectFoodUpgradeData.FoodCount = 0;

        // 3. 시스템 초기화 순서 명확히 하기
        try
        {
            // 사운드 먼저 로드
            SoundPlayer.Instance.Load();

            // 각 시스템 순차적 초기화


            // 비동기 작업 완료 보장을 위해 짧은 딜레이 추가
            GameRoot.Instance.WaitTimeAndCallback(0.1f, () =>
            {
                GameRoot.Instance.InGameSystem.Create();

                GameRoot.Instance.WaitTimeAndCallback(0.1f, () =>
                {

                    // 나머지 초기화
                    GameRoot.Instance.TutorialSystem.ClearRegisiter();
                    GameRoot.Instance.UserData.Money.Value = 0;
                    GameRoot.Instance.UserData.Save();

                    if (!Init)
                    {
                        // 새 스테이지를 시작하기 전에 광고 관련 리소스 해제
                        try
                        {
                            var adManager = GameObject.FindObjectOfType<AdManager>();
                            if (adManager != null)
                            {
                                // 광고 관련 작업을 스테이지 로드 후로 지연
                                adManager.gameObject.SendMessage("PauseAdOperations", true, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning("AdManager 접근 중 오류 발생: " + e.Message);
                        }

                        StartGame(GameRoot.Instance.CurInGameType, LoadCallBack);
                    }
                });
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error during NextGameStage: " + e.ToString());

            // 오류 발생 시 안전하게 초기화 시도
            if (!Init)
            {
                GameRoot.Instance.WaitTimeAndCallback(0.5f, () =>
                {
                    StartGame(GameRoot.Instance.CurInGameType, LoadCallBack);
                });
            }
        }
    }


    public void LoadCallBack()
    {
        GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, 10);

        SoundPlayer.Instance.PlayBGM("bgm", true);

        // 스테이지 로드 완료 후 광고 관련 작업 재개
        try
        {
            var adManager = GameObject.FindObjectOfType<AdManager>();
            if (adManager != null)
            {
                // 1초 후 광고 관련 작업 재개
                GameRoot.Instance.WaitTimeAndCallback(1f, () =>
                {
                    adManager.gameObject.SendMessage("PauseAdOperations", false, SendMessageOptions.DontRequireReceiver);
                });
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("AdManager 재개 중 오류 발생: " + e.Message);
        }
    }

    public bool inInitPopups { get; private set; } = false;

    public void InitPopups()
    {
        disposables.Clear();
        var ActionQueue = new Queue<System.Action>();
        IsActiveEventTuto = false;
        inInitPopups = true;


        NextActionClear = () =>
        {
            inInitPopups = false;
            ActionQueue.Clear();
        };

        System.Action NextAction = () =>
        {

            if (ActionQueue.Count < 1)
            {
                inInitPopups = false;

                return;
            }

            var action = ActionQueue.Dequeue();
            action.Invoke();
        };



        // if (diff.TotalSeconds > minRewardTime && time != DateTime.MinValue)
        // {
        //     int rewardTime = (int)diff.TotalSeconds;

        //     if ((int)diff.TotalSeconds >= maxRewardTime)
        //     {
        //         rewardTime = maxRewardTime;

        //         ActionQueue.Enqueue(() =>
        //         {
        //             if (!GameRoot.Instance.TutorialSystem.IsActive())
        //                 GameRoot.Instance.UISystem.OpenUI<PopupOfflineReward>(popup => popup.Set(maxRewardTime), () => NextAction());
        //         });
        //     }
        //     else
        //     {
        //         ActionQueue.Enqueue(() =>
        //         {
        //             if(!GameRoot.Instance.TutorialSystem.IsActive())
        //             GameRoot.Instance.UISystem.OpenUI<PopupOfflineReward>(popup => popup.Set((int)diff.TotalSeconds), () => NextAction()); //offline not max value
        //         });
        //     }
        // }


        if (!firstInit)
        {
            firstInit = true;
            GameRoot.Instance.Loading.Hide(true, () =>
            {
                NextAction();
            });
        }

        NextAction();
        nextStage = false;

    }




}
