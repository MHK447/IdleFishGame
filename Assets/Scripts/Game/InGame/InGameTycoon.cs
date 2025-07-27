using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using UniRx;
using System.Linq;
using UnityEngine.AI;
using NavMeshPlus.Components;
using Unity.VisualScripting;

public class InGameTycoon : InGameMode
{
    public IReactiveProperty<bool> MaxMode = new ReactiveProperty<bool>(true);

    private int ProductHeroIdxs = 0;
    public override void Load()
    {
        base.Load();
        GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().StartGame();
    }


    public void StartGame()
    {
    }



    protected override void LoadUI()
    {
        base.LoadUI();

        GameRoot.Instance.InGameSystem.InitPopups();
        GameRoot.Instance.UISystem.OpenUI<HudTotal>();
    }



    public override void UnLoad()
    {
        base.UnLoad();
    }
}
