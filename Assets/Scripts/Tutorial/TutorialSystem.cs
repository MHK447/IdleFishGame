using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.AddressableAssets;
using System.Linq;

public enum TutorialIdent
{
    None,
    NextStageBtn,
}

public class TutorialSystem
{
    public class Map
    {
        public string id;
        public GameObject inst;
    }
    Dictionary<TutorialIdent, TutorialRegister> registerDic = new Dictionary<TutorialIdent, TutorialRegister>();

    private List<Map> scenarioMap = new List<Map>();
    private Dictionary<string, string> LoadMapData = new Dictionary<string, string> {
        { Tuto_1 , "Tutorial_1" },
        { Tuto_2 , "Tutorial_2" },
        { Tuto_3 , "Tutorial_3" },
        { Tuto_4 , "Tutorial_4" },
        { Tuto_5 , "Tutorial_5" },
    };

    public const string Tuto_1 = "1";
    public const string Tuto_2 = "2";
    public const string Tuto_3 = "3";
    public const string Tuto_4 = "4";
    public const string Tuto_5 = "5";


    List<GameObject> Active_ScenarioMap = new List<GameObject>();
    public System.Action OnActiveTutoEnd = null;
    public string ActiveTutorialKey = string.Empty;
    public ReactiveProperty<bool> TruckSleepCheckProperty = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> GetVipTicket = new ReactiveProperty<bool>(false);
    public ReactiveProperty<int> UnlockContents = new ReactiveProperty<int>(0);

    public bool IsStartArtifactEqipTutorial = false;

    public void Init()
    {
        foreach (var mapInfo in LoadMapData)
        {
            Addressables.InstantiateAsync(mapInfo.Value).Completed += (handle) =>
            {
                scenarioMap.Add(new Map()
                {
                    id = mapInfo.Key,
                    inst = handle.Result
                });
            };
        }
    }

    public bool IsActive(string compareKey = "")
    {
        if (string.IsNullOrEmpty(compareKey))
            return Active_ScenarioMap.Count > 0;
        else
        {
            if (Active_ScenarioMap.Count > 0 && ActiveTutorialKey == compareKey)
            {
                return true;
            }

            return false;
        }
    }

    public bool IsClearTuto(string key)
    {
        return GameRoot.Instance.UserData.Tutorial.Contains(key);
    }

    public void ClearRegisiter()
    {
        registerDic.Clear();
    }

    public void RemoveRegister(TutorialIdent _index)
    {
        if (registerDic.ContainsKey(_index))
        {
            registerDic.Remove(_index);
        }
    }

    public void AddRegister(TutorialIdent _index, TutorialRegister _register)
    {
        if (!registerDic.ContainsKey(_index))
        {
            registerDic.Add(_index, _register);
        }
    }

    public TutorialRegister GetRegister(TutorialIdent _index)
    {
        if (registerDic.ContainsKey(_index))
            return registerDic[_index];
        else
            return null;
    }

    public void InitTuto(string key)
    {
        if (GameRoot.Instance.UserData.Tutorial.Contains(key))
        {
            GameRoot.Instance.UserData.Tutorial.Remove(key);
            GameRoot.Instance.UserData.Save();
        }
    }

    public void AllTutorialInit()
    {
        var count = GameRoot.Instance.UserData.Tutorial.Count;

        for (int i = 0; i < count; ++i)
        {
            if (GameRoot.Instance.UserData.Tutorial.Contains(GameRoot.Instance.UserData.Tutorial[i]))
            {
                GameRoot.Instance.UserData.Tutorial.Remove(GameRoot.Instance.UserData.Tutorial[i]);
                GameRoot.Instance.UserData.Save();
            }
        }
    }

    public void StartTutorial(string _key, bool initCallback = false)
    {
        if (initCallback) OnActiveTutoEnd = null;

        if (!GameRoot.Instance.UserData.Tutorial.Contains(_key))
        {
            GameRoot.Instance.UserData.Tutorial.Add(_key);
            GameRoot.Instance.UserData.Save();
        }
        else
        {
            OnActiveTutoEnd?.Invoke();
            return;
        }

        ActiveTutorialKey = _key;
        if (LoadMapData.ContainsKey(_key))
        {
            Addressables.InstantiateAsync(LoadMapData[_key], GameRoot.Instance.UISystem.UIRootT.transform, false).Completed += (handle) =>
            {
                if (Active_ScenarioMap.Count < 1)
                {
                    var tutomap = handle.Result.GetComponent<TutorialMap>();
                    if (tutomap != null)
                    {
                        tutomap.StartMap();
                    }
                    // var tutoguidepopup = handle.Result.GetComponent<PopupTutorialGuide>();
                    // if(tutoguidepopup != null)
                    // {
                    //     tutoguidepopup.StartTutorial(_key);
                    // }
                }

                Active_ScenarioMap.Add(handle.Result);
            };
        }
    }

    public void TutoClear(string _key)
    {
        if (!GameRoot.Instance.UserData.Tutorial.Contains(_key))
        {
            GameRoot.Instance.UserData.Tutorial.Add(_key);
            GameRoot.Instance.UserData.Save();
        }
    }

    public void EndTuto()
    {
        if (Active_ScenarioMap.Count > 0)
        {
            if (!Addressables.ReleaseInstance(Active_ScenarioMap[0]))
                GameObject.Destroy(Active_ScenarioMap[0]);
            Active_ScenarioMap.RemoveAt(0);

            if (Active_ScenarioMap.Count > 0)
            {
                var tutomap = Active_ScenarioMap[0].GetComponent<TutorialMap>();
                if (tutomap != null)
                {
                    tutomap.StartMap();
                }
            }
            else
            {
                ActiveTutorialKey = string.Empty;
                OnActiveTutoEnd?.Invoke();
                OnActiveTutoEnd = null;
            }
        }
        else
        {
            ActiveTutorialKey = string.Empty;
            OnActiveTutoEnd?.Invoke();
            OnActiveTutoEnd = null;
        }

    }


    public GameObject GetTarget(TutorialIdent id, int targetIdx = -1, int targetSubIdx = -1)
    {
        switch (id)
        {
            //case TutorialIdent.WorkplaceBuildStartBtn:
            //    {
            //        return GameRoot.Instance.InGameSystem.GetInGame<InGameLumber>().Stage.WorkSpace.GetTutoBuildStartBtn();
            //    }

            //case TutorialIdent.WorkplaceBuildCompBtn:
            //    {
            //        return GameRoot.Instance.InGameSystem.GetInGame<InGameLumber>().Stage.WorkSpace.GetTutoBuildCompBtn();
            //    }
                // case TutorialIdent.HUD_ShopBtn:
                //     {
                //         // var getui = GameRoot.Instance.UISystem.GetUI<HUDTotal>();
                //         // if (getui != null)
                //         // {
                //         //     return getui.GetShopBtnObj;
                //         // }
                //     }
                //     break;
                // case TutorialIdent.PiggyBtn:
                //     {
                //         // var getui = GameRoot.Instance.UISystem.GetUI<HUDTotal>();
                //         // if (getui != null)
                //         // {
                //         //     return getui.GetPiggyBankBtnObj;
                //         // }
                //     }
                //     break;
                // case TutorialIdent.InGameManagerSlot:
                //     {
                //         var getclinic = GameRoot.Instance.InGameSystem.GetInGame<InGameRestaurant>().CurInGameStage.GetClinicList().FirstOrDefault();

                //         if (getclinic != null)
                //         {
                //             return getclinic.InGameManagerSlot.gameObject;
                //         }
                //     }
                //     break;
                // case TutorialIdent.InGameManagerSlotFocusTrans:
                //     {
                //         var getclinic = GameRoot.Instance.InGameSystem.GetInGame<InGameRestaurant>().CurInGameStage.GetClinicList().FirstOrDefault();

                //         if(getclinic != null)
                //         {
                //             return getclinic.GetManagerSlotTr.gameObject;
                //         }
                //     }
                //     break;
                // case TutorialIdent.PopupUpgrade_FirstBtn:
                //     {
                //         // var ui = GameRoot.Instance.UISystem.GetUI<PopupUpgrades>();
                //         // if (ui != null)
                //         // {
                //         //     return ui.GetItem(targetIdx);
                //         // }
                //     }
                //     break;
                // case TutorialIdent.ChapterPass_RegionBtn:
                //     {
                //         var popup = GameRoot.Instance.UISystem.GetUI<PopupChapterPass>();
                //         if (popup != null)
                //         {
                //             return popup.GetCurrentPlayRegion();
                //         }
                //     }
                //     break;
                // case TutorialIdent.ChapterPass_NextBtn:
                //     {
                //         if (GameRoot.Instance.UserData.CurMode.StageData.StageIdx.Value % 7 != 0)
                //         {
                //             var register = GameRoot.Instance.TutorialSystem.GetRegister(TutorialIdent.HUD_NextStageBtn);
                //             return register.gameObject;
                //         }
                //         else
                //         {
                //             var register = GameRoot.Instance.TutorialSystem.GetRegister(TutorialIdent.ChapterPass_NextBtn);
                //             return register.gameObject;
                //         }
                //     }
                // case TutorialIdent.ActiveTip:
                //     {
                //         var tip = GameRoot.Instance.InGameSystem.GetInGame<InGameRestaurant>().CurInGameStage.FindActiveTip();
                //         return tip.gameObject;
                //     }
                //     break;

                //case TutorialIdent.SewingEventCustomer:
                //    {
                //        return GameRoot.Instance.InGameSystem.GetInGame<InGameRestaurant>().GetEventCustomer().gameObject;
                //    }
                //    break;
        }
        return null;
    }

    public bool IsDynamaicTarget(TutorialIdent id)
    {
        //switch (id)
        //{
        //    case TutorialIdent.WorkplaceBuildStartBtn:
        //    case TutorialIdent.WorkplaceBuildCompBtn:
        //        return true;

        //    default:
        //        return false;
        //}
        return false;
    }
    
}
