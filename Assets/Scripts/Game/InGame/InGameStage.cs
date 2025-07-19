using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UniRx;
using DG.Tweening;
using System.Linq;
using BanpoFri;

public enum CasherType
{
    CounterCasher = 1,
    CarryCasher = 2,
    FishingCasher = 3,
}

public class InGameStage : MonoBehaviour
{
    public bool IsLoadComplete { get; private set; }

    private CompositeDisposable disposable = new CompositeDisposable();


    public void Init()
    {
        IsLoadComplete = false;
        disposable.Clear();
    }


    public void ReturnMainScreen()
    {
        GameRoot.Instance.UserData.CurMode.GachaCoin.Value = 0;
    }

    private void OnDestroy()
    {
        disposable.Clear();
    }


    
}
