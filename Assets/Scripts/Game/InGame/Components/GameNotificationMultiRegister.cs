using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GameNotificationMultiRegister : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        public bool awakeRegit = false;
        public GameNotificationSystem.NotificationCategory category;
        public int targetIdx = -1;
        public int targetSubIdx = -1;
        public bool reverse = false;
        [HideInInspector]
        public bool on = false;
        public IDisposable disposables = null;
    }
    [SerializeField]
    private GameObject redDot;
    [SerializeField]
    private List<Data> listNoti = new List<Data>();
    private CompositeDisposable disposables = new CompositeDisposable();

    private void Awake()
    {
        foreach (var data in listNoti)
        {
            if (data.awakeRegit)
            {
                var noti = GameRoot.Instance.GameNotification.GetData(data.category, data.targetIdx, data.targetSubIdx);
                if (noti != null)
                {
                    noti.on.Subscribe(x =>
                    {
                        if (data.reverse)
                        {
                            data.on = !x;
                        }
                        else
                        {
                            data.on = x;
                        }
                        UpdateActive();
                    }).AddTo(this);

                    UpdateActive();
                }
            }
        }
    }

    public void Init(GameNotificationSystem.NotificationCategory category, int _targetIdx, int _targetSubIdx)
    {
        var find = listNoti.Find(x => x.category == category);
        if (find != null)
        {
            if (!find.awakeRegit)
            {
                if (find.disposables != null)
                {
                    if (!disposables.Remove(find.disposables))
                        find.disposables.Dispose();
                    find.disposables = null;
                }

                var noti = GameRoot.Instance.GameNotification.GetData(find.category, find.targetIdx, find.targetSubIdx);
                if (noti != null)
                {
                    find.disposables = noti.on.Subscribe(x =>
                    {
                        if (find.reverse)
                        {
                            find.on = !x;
                        }
                        else
                        {
                            find.on = x;
                        }
                        UpdateActive();
                    }).AddTo(disposables);
                }

                UpdateActive();
            }
        }
    }

    public void Add(GameNotificationSystem.NotificationCategory _category, int _targetIdx, int _targetSubIdx, bool _reverse)
    {
        var newNoti = new Data()
        {
            category = _category,
            targetIdx = _targetIdx,
            targetSubIdx = _targetSubIdx,
            awakeRegit = false,
            reverse = _reverse
        };
        listNoti.Add(newNoti);

        if(newNoti.disposables != null)
        {
            if (!disposables.Remove(newNoti.disposables))
                newNoti.disposables.Dispose();
            newNoti.disposables = null;
        }
        var noti = GameRoot.Instance.GameNotification.GetData(newNoti.category, newNoti.targetIdx, newNoti.targetSubIdx);
        if (noti != null)
        {
            newNoti.disposables = noti.on.Subscribe(x =>
            {
                if (newNoti.reverse)
                {
                    newNoti.on = !x;
                }
                else
                {
                    newNoti.on = x;
                }
                UpdateActive();
            }).AddTo(disposables);
        }

        UpdateActive();
    }

    public void UpdateActive()
    {
        var totalActive = false;
        foreach (var data in listNoti)
        {
            if (data.on)
            {
                totalActive = true;
                break;
            }
        }
        ProjectUtility.SetActiveCheck(redDot, totalActive);
    }

    public void UpdateRedDotPos(Vector3 worldPos)
    {
        redDot.transform.position = worldPos;
    }

    private void OnDestroy()
    {
        disposables.Clear();
    }
}
