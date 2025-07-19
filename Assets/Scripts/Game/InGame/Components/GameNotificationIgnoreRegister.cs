using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GameNotificationIgnoreRegister : MonoBehaviour
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
    private Data notiData = null;
    [SerializeField]
    private Data ignoreData = null;
    private CompositeDisposable disposables = new CompositeDisposable();

    private void Awake()
    {
        if (notiData.awakeRegit)
        {
            var noti = GameRoot.Instance.GameNotification.GetData(notiData.category, notiData.targetIdx, notiData.targetSubIdx);
            if (noti != null)
            {
                noti.on.Subscribe(x =>
                {
                    if (notiData.reverse)
                    {
                        notiData.on = !x;
                    }
                    else
                    {
                        notiData.on = x;
                    }
                    UpdateActive();
                }).AddTo(this);

                UpdateActive();
            }
        }

        if (ignoreData.awakeRegit)
        {
            var noti = GameRoot.Instance.GameNotification.GetData(ignoreData.category, ignoreData.targetIdx, ignoreData.targetSubIdx);
            if (noti != null)
            {
                noti.on.Subscribe(x =>
                {
                    if (ignoreData.reverse)
                    {
                        ignoreData.on = !x;
                    }
                    else
                    {
                        ignoreData.on = x;
                    }
                    UpdateActive();
                }).AddTo(this);

                UpdateActive();
            }
        }
        
    }

    public void Init()
    {
        if (!ignoreData.awakeRegit)
        {
            var noti = GameRoot.Instance.GameNotification.GetData(ignoreData.category, ignoreData.targetIdx, ignoreData.targetSubIdx);
            if (noti != null)
            {
                noti.on.Subscribe(x =>
                {
                    if (ignoreData.reverse)
                    {
                        ignoreData.on = !x;
                    }
                    else
                    {
                        ignoreData.on = x;
                    }
                    UpdateActive();
                }).AddTo(this);

                UpdateActive();
            }
        }

        if (!notiData.awakeRegit)
        {
            if (notiData.disposables != null)
            {
                if (!disposables.Remove(notiData.disposables))
                    notiData.disposables.Dispose();
                notiData.disposables = null;
            }

            var noti = GameRoot.Instance.GameNotification.GetData(notiData.category, notiData.targetIdx, notiData.targetSubIdx);
            if (noti != null)
            {
                notiData.disposables = noti.on.Subscribe(x =>
                {
                    if (notiData.reverse)
                    {
                        notiData.on = !x;
                    }
                    else
                    {
                        notiData.on = x;
                    }
                    UpdateActive();
                }).AddTo(disposables);
            }

            UpdateActive();
        }
    }

    public void UpdateActive()
    {
        var totalActive = notiData.on;
        if (notiData.on)
        {
            var ignore = GameRoot.Instance.GameNotification.GetData(ignoreData.category, ignoreData.targetIdx, ignoreData.targetSubIdx);
            if (ignore.on.Value)
                totalActive = false;
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
