using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UniRx;

public class HudNoticeComponents : MonoBehaviour
{
    private CompositeDisposable disposables = new CompositeDisposable();

    public void Init()
    {
        GameRoot.Instance.UserData.CurMode.NoticeCollections.Clear();
        disposables.Clear();
        GameRoot.Instance.UserData.CurMode.NoticeCollections.ObserveAdd().Subscribe(x =>
        {
            CheckNoti();
        }).AddTo(disposables);


        GameRoot.Instance.UserData.CurMode.NoticeCollections.ObserveRemove().Subscribe(x =>
        {
            CheckNoti();
        }).AddTo(disposables);

        CheckNoti();
    }

    public void CheckNoti()
    {
    }

    public void NoticeClear()
    {
    }

    void OnDisable()
    {
    }
}
