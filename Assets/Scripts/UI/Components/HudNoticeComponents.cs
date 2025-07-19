using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UniRx;

public class HudNoticeComponents : MonoBehaviour
{
    [SerializeField]
    private List<NoticeComponent> NoticeComponentList = new List<NoticeComponent>();

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
        foreach (var notice in NoticeComponentList)
        {
            ProjectUtility.SetActiveCheck(notice.gameObject, false);
        }

        for (int i = 0; i < GameRoot.Instance.UserData.CurMode.NoticeCollections.Count; ++i)
        {
            ProjectUtility.SetActiveCheck(NoticeComponentList[i].gameObject, true);
            NoticeComponentList[i].Set((NoticeComponent.NoticeType)GameRoot.Instance.UserData.CurMode.NoticeCollections[i].NotiIdx, GameRoot.Instance.UserData.CurMode.NoticeCollections[i].Target);
        }
    }

    public void NoticeClear()
    {
        foreach (var notice in NoticeComponentList)
        {
            ProjectUtility.SetActiveCheck(notice.gameObject, false);
        }
    }

    void OnDisable()
    {
        foreach (var notice in NoticeComponentList)
        {
            ProjectUtility.SetActiveCheck(notice.gameObject, false);
        }
    }
}
