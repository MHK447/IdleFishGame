using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class DoTweenScriptComponent : MonoBehaviour
{
    private enum Type
    {
        Position,
        LocalPosition,
        Alpha,
        MoveY,
        MoveX,
        DelayCallback,
        LocalMoveY,
        LocalMoveX,
        Scale,
        ScaleY,
        ScaleX,
        Rotation,
    }
    private enum Loop
    {
        Once,
        Restart,
        Yoyo
    }


    [System.Serializable]
    private class DoTweenInfo
    {
        public Type TweenType;
        public float Value;
        public Vector3 To;
        public float Duration = 0;
        public float Delay = 0;
        public Ease EaseType;
        public bool customGraph;
        public AnimationCurve customCurve;
        public UnityEvent Callback;
    }

    [SerializeField]
    private List<DoTweenInfo> Tweens = new List<DoTweenInfo>();

    [SerializeField]
    private Loop LoopType = Loop.Once;
    [SerializeField]
    private bool AutoPlay = true;

    [SerializeField]
    private bool OnEnablePlay = false;

    [SerializeField]
    private bool IsUnScaledTime = false;

    private Sequence seq;

    private bool IsOnDisableCheck = false;


    private void Awake()
    {
        Init();
    }


    public void Init()
    {
        seq = DOTween.Sequence();
        for (int i = 0; i < Tweens.Count; i++)
        {
            var idx = i;
            switch (Tweens[idx].TweenType)
            {
                case Type.Position:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOMove(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOMove(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;

                case Type.LocalPosition:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOLocalMove(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOLocalMove(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.MoveY:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOMoveY(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOMoveY(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.MoveX:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOMoveX(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOMoveX(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.LocalMoveY:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOLocalMoveY(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOLocalMoveY(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.LocalMoveX:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOLocalMoveX(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOLocalMoveX(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.Alpha:
                    {
                        TweenParams tweenParams = new TweenParams();
                        if (Tweens[i].customGraph) { tweenParams.SetEase(Tweens[idx].customCurve); }
                        else { tweenParams.SetEase(Tweens[idx].EaseType); }

                        Image img = GetComponent<Image>();
                        if (img != null)
                        {
                            seq.AppendInterval(Tweens[idx].Delay).Append(img.DOFade(Tweens[idx].Value, Tweens[idx].Duration).SetAs(tweenParams)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                            break;
                        }

                        SpriteRenderer spRen = GetComponent<SpriteRenderer>();
                        if (spRen != null)
                        {
                            seq.AppendInterval(Tweens[idx].Delay).Append(spRen.DOFade(Tweens[idx].Value, Tweens[idx].Duration).SetAs(tweenParams)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                            break;
                        }

                        Text text = GetComponent<Text>();
                        if (text != null)
                        {
                            seq.AppendInterval(Tweens[idx].Delay).Append(text.DOFade(Tweens[idx].Value, Tweens[idx].Duration).SetAs(tweenParams)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                            break;
                        }

                        CanvasGroup canvas = GetComponent<CanvasGroup>();
                        if (canvas != null)
                        {
                            seq.AppendInterval(Tweens[idx].Delay).Append(canvas.DOFade(Tweens[idx].Value, Tweens[idx].Duration).SetAs(tweenParams)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                            break;
                        }

                    }
                    break;
                case Type.Scale:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOScale(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOScale(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.ScaleX:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOScaleX(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DOScaleX(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.ScaleY:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[i].Delay).Append(this.transform.DOScaleY(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[i].Delay).Append(this.transform.DOScaleY(Tweens[idx].Value, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.Rotation:
                    {
                        if (!Tweens[idx].customGraph)
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DORotate(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].EaseType)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                        else
                            seq.AppendInterval(Tweens[idx].Delay).Append(this.transform.DORotate(Tweens[idx].To, Tweens[idx].Duration).SetEase(Tweens[idx].customCurve)).AppendCallback(() => {
                                Tweens[idx].Callback?.Invoke();
                            }).SetUpdate(IsUnScaledTime);
                    }
                    break;
                case Type.DelayCallback:
                    {
                        seq.AppendInterval(Tweens[idx].Delay).AppendCallback(() => { Tweens[idx].Callback?.Invoke(); }).AppendInterval(Tweens[idx].Duration).SetUpdate(IsUnScaledTime);
                    }
                    break;
            }
        }

        if (Tweens.Count > 0)
        {
            if (LoopType == Loop.Restart)
                seq.SetLoops(-1, DG.Tweening.LoopType.Restart);
            else if (LoopType == Loop.Yoyo)
                seq.SetLoops(-1, DG.Tweening.LoopType.Yoyo);




            if (AutoPlay)
                seq.Play();
            else
                seq.Pause();
        }

        seq.SetAutoKill(false);
    }

    public void SetId(string id)
    {
        seq.SetId(id);
    }

    public void OnEnable()
    {
        if(seq != null && OnEnablePlay)
        {
            seq.Restart();
        }
    }

    public void AddCallback(System.Action action)
    {
        if(seq != null)
        {
            seq.AppendCallback(() => {
                action?.Invoke();
            });
        }
        else
        {
            Tweens.Last().Callback.AddListener(() => { action.Invoke(); });
        }
    }

    public void Play()
    {
        if(seq != null)
        {
            seq.Play();
        }
    }

    public void Restart()
    {
        if (seq != null)
        {
            seq.Restart();
        }
    }

    public void InitStart()
    {
        if (seq != null)
        {
            seq.Restart();
            seq.Play();
        }

    }

    public void InitPause()
    {
        if(seq != null)
        {
            seq.Restart();
            seq.Pause();
        }

    }
    public void Pause()
    {
        if(seq != null)
        {
            seq.Pause();
        }
    }

    private void OnDestroy()
    {
        seq.Kill();
    }

    private void OnDisable()
    {
        if(IsOnDisableCheck)
        seq.Kill();
    }




    // 두트윈 에디터 전용 Preview
    #region "Dotween Preview Editor"

    public Sequence GetSequence(){ return seq; }
    public void InitSequence(){ 
        if (seq != null){ seq.Kill(); }
        Awake(); 
    }
#endregion



}
