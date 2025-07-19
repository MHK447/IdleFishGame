using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameNotificationRegister : MonoBehaviour
{
    [SerializeField]
    private bool awakeRegit = false;
    [SerializeField]
    private GameNotificationSystem.NotificationCategory category;
    [SerializeField]
    private int targetIdx = -1;
    [SerializeField]
    private int targetSubIdx = -1;
    [SerializeField]
    private GameObject redDot;
    [SerializeField]
    private Text reddotCount;

    [SerializeField]
    private bool reverse = false;
    [SerializeField]
    private bool anyCheckInCategory = false;

    private CompositeDisposable disposables = new CompositeDisposable();


    private CompositeDisposable nulldisposables = new CompositeDisposable();

    private void Awake()
    {
        if (awakeRegit)
        {
            disposables.Clear();
            var noti = GameRoot.Instance.GameNotification.GetData(category, targetIdx, targetSubIdx);
            if (noti != null)
            {
                if (reverse)
                {
                    noti.on.Subscribe(x => ProjectUtility.SetActiveCheck(redDot, !x)).AddTo(disposables);
                }
                else
                {
                    noti.on.Subscribe(x => ProjectUtility.SetActiveCheck(redDot, x)).AddTo(disposables);
                }
                if (reddotCount != null) noti.onCount.Subscribe(x => reddotCount.text = x.ToString()).AddTo(disposables);
            }
            else
            {
                   GameRoot.Instance.GameNotification.GetNotifications.ObserveAdd().Subscribe(x => {
                    if(x.Key == category)
                    {
                        nulldisposables.Clear();

                        var noti = GameRoot.Instance.GameNotification.GetData(category, targetIdx, targetSubIdx);

                        if (reverse)
                        {
                            noti.on.Subscribe(y => ProjectUtility.SetActiveCheck(redDot, !y)).AddTo(disposables);
                        }
                        else
                        {
                            noti.on.Subscribe(y => ProjectUtility.SetActiveCheck(redDot, y)).AddTo(disposables);
                        }
                        if (reddotCount != null) noti.onCount.Subscribe(x => reddotCount.text = x.ToString()).AddTo(disposables);
                    }
                }).AddTo(nulldisposables);
            }

            if(anyCheckInCategory)
            {
                if(GameRoot.Instance.GameNotification.GetNotifications.ContainsKey(category))
                {
                    var notiData = GameRoot.Instance.GameNotification.GetNotifications[category];
                    
                    System.Action checkAnyNoti = () => {
                        bool result = false;
                        foreach(var checkNoti in notiData)
                        {
                            if(checkNoti.on.Value)
                                result = true;
                        }

                        ProjectUtility.SetActiveCheck(redDot, reverse ? !result : result);
                    };  
                    foreach(var no in notiData)
                    {
                        no.on.SkipLatestValueOnSubscribe().Subscribe(x => {
                            checkAnyNoti();
                        }).AddTo(disposables);
                    }
                    checkAnyNoti();
                }
            }

            UpdateActive();
        }
    }

    public void Init(int _targetIdx, int _targetSubIdx)
    {
        targetIdx = _targetIdx;
        targetSubIdx = _targetSubIdx;
        if (!awakeRegit)
        {
            disposables.Clear();
            var noti = GameRoot.Instance.GameNotification.GetData(category, targetIdx, targetSubIdx);
            if (noti != null)
            {
                if (reverse)
                {
                    noti.on.Subscribe(x => ProjectUtility.SetActiveCheck(redDot, !x)).AddTo(disposables);
                }
                else
                {
                    noti.on.Subscribe(x => ProjectUtility.SetActiveCheck(redDot, x)).AddTo(disposables);
                }
                if (reddotCount != null) noti.onCount.Subscribe(x => reddotCount.text = x.ToString()).AddTo(disposables);
            }

            UpdateActive();
        }
    }

    public void Init(GameNotificationSystem.NotificationCategory _category, int _targetIdx, int _targetSubIdx, bool _reverse = false)
    {
        category = _category;
        targetIdx = _targetIdx;
        targetSubIdx = _targetSubIdx;
        reverse = _reverse;
        if (!awakeRegit)
        {
            disposables.Clear();
            var noti = GameRoot.Instance.GameNotification.GetData(category, targetIdx, targetSubIdx);
            if (noti != null)
            {
                if (reverse)
                {
                    noti.on.Subscribe(x => ProjectUtility.SetActiveCheck(redDot, !x)).AddTo(disposables);
                }
                else
                {
                    noti.on.Subscribe(x => ProjectUtility.SetActiveCheck(redDot, x)).AddTo(disposables);
                }
                if (reddotCount != null) noti.onCount.Subscribe(x => reddotCount.text = x.ToString()).AddTo(disposables);
            }
        }
    }

    public void SetReddot(GameObject reddot)
    {
        redDot = reddot;
    }

    public void UpdateRedDotPos(Vector3 worldPos)
    {
        redDot.transform.position = worldPos;
    }

    public void SetCategory(GameNotificationSystem.NotificationCategory _category)
    {
        category = _category;
    }

    private void OnDestroy()
    {
        disposables.Clear();
    }

    public void RemoveRegist()
    {
        disposables.Clear();
        nulldisposables.Clear();

        targetIdx = -1;
        targetSubIdx = -1;

        ProjectUtility.SetActiveCheck(redDot, false);
    }

    public void UpdateActive()
    {
        var noti = GameRoot.Instance.GameNotification.GetData(category, targetIdx, targetSubIdx);
        if (noti != null)
        {
            if (reverse)
            {
                ProjectUtility.SetActiveCheck(redDot, !noti.on.Value);
            }
            else
            {
                ProjectUtility.SetActiveCheck(redDot, noti.on.Value);
            }
            if (reddotCount != null) reddotCount.text = noti.onCount.Value.ToString();
        }
    }
}
