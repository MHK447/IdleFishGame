using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BanpoFri;
using UnityEngine.AddressableAssets;
using UniRx;
public class GoodEffect : MonoBehaviour
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Image Bg;
    [SerializeField]
    private Image Frame;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float firstMoveTime = 0.25f;
    [SerializeField]
    private float secondMoveTime = 0.75f;
    [SerializeField]
    private float firstEffectTime = 1f;
    [SerializeField]
    private Vector3 iconIdleScale;
    [SerializeField]
    private Text itemCnt;
    [SerializeField]
    private GameObject hitEffect;
    [SerializeField]
    private Text titleText;

    public void Set(
        Vector3 worldStartPos,
        Vector3 worldMiddlePos,
        Vector3 worldEndPos,
        int goodsType,
        int goodsIdx,
        int goodsGrade,
        System.Numerics.BigInteger goodsCnt,
        Action OnEnd = null,
        float delay = 0f,
        string viewText = "",
        bool isreward = true,
        string aniName = "Show")
    {
        StartCoroutine(WaitFrame());
        this.transform.position = worldStartPos;

        ProjectUtility.SetActiveCheck(icon.gameObject, true);
        icon.sprite = ProjectUtility.GetRewardItemIconImg(goodsType , goodsIdx , goodsGrade);
        icon.SetNativeSize();

        if (titleText != null)
        {
            titleText.gameObject.SetActive(viewText.Length > 0);
            if (viewText.Length > 0)
            {
                titleText.text = viewText;
            }
        }

        if (itemCnt != null)
        {
            if (goodsType == (int)Config.RewardType.Currency && ((int)1 == goodsIdx))
            {
                string money = ProjectUtility.CalculateMoneyToString(goodsCnt);
                itemCnt.text = money;
            }
            else if (goodsCnt == 0)
            {
                itemCnt.text = "";
            }
            else
            {
                itemCnt.text = goodsCnt.ToString();
            }
        }
        animator.Play(aniName, 0, 0f);
        var firstMove = DOTween.To(() => this.transform.position, x =>
        {
            this.transform.position = x;
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
        }, worldMiddlePos, firstMoveTime);
        firstMove.SetEase(Ease.InQuart);
        var secondMove = DOTween.To(() => this.transform.position, x =>
        {
            this.transform.position = x;
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
        }, worldEndPos, secondMoveTime);

        secondMove.SetEase(Ease.InQuart);
        secondMove.OnStart(() => { animator.Play("Hide", 0, 0f); });
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(firstMove);
        sequence.AppendCallback(() =>
        {
            //animator.Play("Hide", 0, 0f);
            //firstmove event
        });
        sequence.AppendInterval(firstEffectTime);

        if (viewText.Length > 0)
        {
            sequence.AppendCallback(() =>
            {
            });
            sequence.AppendInterval(1.5f);
        }
        sequence.Append(secondMove);
        sequence.AppendCallback(() =>
        {
            if (hitEffect != null)
            {
                hitEffect.SetActive(true);
                icon.gameObject.transform.localScale = iconIdleScale;
                itemCnt.gameObject.SetActive(false);
            }
            if (Bg != null) Bg.gameObject.SetActive(false);
            if (Frame != null) Frame.gameObject.SetActive(false);

            OnEnd?.Invoke();

            //SoundPlayer.Instance.PlaySound("get");

            if (goodsType == (int)Config.RewardType.Currency)
            {
                GameRoot.Instance.UserData.SyncHUDCurrency(goodsIdx);
            }

            CompositeDisposable disposables = new CompositeDisposable();
            var startcount = GameRoot.Instance.PlayTimeSystem.CreateCountDownObservable(1f);
            startcount.Subscribe(_ => {; }, () =>
            {
                disposables.Clear();
                if (!Addressables.ReleaseInstance(this.gameObject))
                    Destroy(this.gameObject);
            }).AddTo(disposables);

        });
    }
    public void SetPlayIcon(
        Vector3 worldStartPos,
        Vector3 worldMiddlePos,
        Vector3 worldEndPos,
        string Icon,
        Action OnEnd = null,
        float delay = 0f,
        string Ani = "Show",
        float firsttime = 0.95f)
    {

        StartCoroutine(WaitFrame());
        this.transform.position = worldStartPos;

        //icon.sprite = AtlasManager.Instance.GetSprite(Atlas.Atals_UI_PopupImg,Icon);
        //icon.SetNativeSize();

        itemCnt.text = "";

        animator.Play(Ani, 0, 0f);
        var firstMove = DOTween.To(() => this.transform.position, x =>
        {
            this.transform.position = x;
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
        }, worldMiddlePos, firsttime);
        firstMove.SetEase(Ease.InQuart);
        var secondMove = DOTween.To(() => this.transform.position, x =>
        {
            this.transform.position = x;
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
        }, worldEndPos, secondMoveTime);

        secondMove.SetEase(Ease.InQuart);
        secondMove.OnStart(() => { animator.Play("Hide", 0, 0f); });
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(firstMove);
        sequence.AppendCallback(() =>
        {
            //animator.Play("Hide", 0, 0f);
            //firstmove event
        });
        sequence.AppendInterval(firstEffectTime);

        sequence.Append(secondMove);
        sequence.AppendCallback(() =>
        {
            if (hitEffect != null)
            {
                hitEffect.SetActive(true);
                icon.gameObject.transform.localScale = iconIdleScale;
                itemCnt.gameObject.SetActive(false);
            }
            if (Bg != null) Bg.gameObject.SetActive(false);
            if (Frame != null) Frame.gameObject.SetActive(false);


            //SoundPlayer.Instance.PlaySound("get");

            CompositeDisposable disposables = new CompositeDisposable();
            var startcount = GameRoot.Instance.PlayTimeSystem.CreateCountDownObservable(1f);
            startcount.Subscribe(_ => {; }, () =>
            {
                disposables.Clear();
                if (!Addressables.ReleaseInstance(this.gameObject))
                    Destroy(this.gameObject);
            }).AddTo(disposables);

            OnEnd?.Invoke();
        });
    }

    public void SetPackage(
        Vector3 worldStartPos,
        Vector3 worldMiddlePos,
        Vector3 worldEndPos,
        int packageIdx,
        Action OnEnd = null,
        float delay = 0f)
    {

        StartCoroutine(WaitFrame());
        this.transform.position = worldStartPos;

        itemCnt.text = "";

        animator.Play("Show", 0, 0f);
        var firstMove = DOTween.To(() => this.transform.position, x =>
        {
            this.transform.position = x;
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
        }, worldMiddlePos, firstMoveTime);
        firstMove.SetEase(Ease.InQuart);
        var secondMove = DOTween.To(() => this.transform.position, x =>
        {
            this.transform.position = x;
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
        }, worldEndPos, secondMoveTime);

        secondMove.SetEase(Ease.InQuart);
        secondMove.OnStart(() => { animator.Play("Hide", 0, 0f); });
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(firstMove);
        sequence.AppendCallback(() =>
        {
            //animator.Play("Hide", 0, 0f);
            //firstmove event
        });
        sequence.AppendInterval(firstEffectTime);

        sequence.Append(secondMove);
        sequence.AppendCallback(() =>
        {
            if (hitEffect != null)
            {
                hitEffect.SetActive(true);
                icon.gameObject.transform.localScale = iconIdleScale;
                itemCnt.gameObject.SetActive(false);
            }
            if (Bg != null) Bg.gameObject.SetActive(false);
            if (Frame != null) Frame.gameObject.SetActive(false);


            SoundPlayer.Instance.PlaySound("get");

            CompositeDisposable disposables = new CompositeDisposable();
            var startcount = GameRoot.Instance.PlayTimeSystem.CreateCountDownObservable(1f);
            startcount.Subscribe(_ => {; }, () =>
            {
                disposables.Clear();
                if (!Addressables.ReleaseInstance(this.gameObject))
                    Destroy(this.gameObject);
            }).AddTo(disposables);

            OnEnd?.Invoke();
        });
    }

    public void SetCurveMove(
        Vector3 worldStartPos,
        Vector3 worldEndPos,
        int goodsType,
        int goodsIdx,
        int goodsGrade,
        Action OnEnd = null,
        float delay = 0f)
    {

        this.gameObject.GetComponent<Animator>().enabled = false;


        StartCoroutine(WaitFrame());

        this.transform.position = worldStartPos;
        this.transform.localScale = Vector3.zero;

        var center = new UnityEngine.Vector3(
        GameRoot.Instance.InGameSystem.CurInGame.CamPixelWidth / 2,
        GameRoot.Instance.InGameSystem.CurInGame.CamPixelHeight / 2, 0);

        itemCnt.text = "";

        if(center.x < worldStartPos.x)
        {
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.Append(this.transform.DOLocalMoveX(-50, 0.4f).SetEase(Ease.OutQuad));
            sequence.Append(DOTween.To(() => this.transform.position.x, x =>
            {
                this.transform.position = new Vector3(x, this.transform.position.y);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
            }, worldEndPos.x, 0.6f).SetEase(Ease.InCubic));
            sequence.AppendCallback(() =>
            {
                if (hitEffect != null)
                {
                    hitEffect.SetActive(true);
                    icon.gameObject.transform.localScale = iconIdleScale;
                    itemCnt.gameObject.SetActive(false);

                }
                if (Bg != null) Bg.gameObject.SetActive(false);
                if (Frame != null) Frame.gameObject.SetActive(false);

                if (icon != null)
                    ProjectUtility.SetActiveCheck(icon.gameObject, false);

                SoundPlayer.Instance.PlaySound("get");

                CompositeDisposable disposables = new CompositeDisposable();
                var startcount = GameRoot.Instance.PlayTimeSystem.CreateCountDownObservable(1f);
                startcount.Subscribe(_ => {; }, () =>
                {
                    disposables.Clear();
                    if (!Addressables.ReleaseInstance(this.gameObject))
                        Destroy(this.gameObject);
                }).AddTo(disposables);

                OnEnd?.Invoke();
            });

            var seq_2 = DOTween.Sequence();
            seq_2.AppendInterval(delay);
            seq_2.Append(DOTween.To(() => this.transform.position.y, y =>
            {
                this.transform.position = new Vector3(this.transform.position.x, y);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
            }, worldEndPos.y, 1f).SetEase(Ease.InQuad));


            var seq_3 = DOTween.Sequence();
            seq_3.AppendInterval(delay);
            seq_3.Append(this.transform.DOScale(3.5f, 0.4f).SetEase(Ease.OutExpo));
            seq_3.Append(this.transform.DOScale(0.3f, 0.6f).SetEase(Ease.InExpo));
        }
        else
        {
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.Append(this.transform.DOLocalMoveY(-40, 0.4f).SetEase(Ease.OutQuad));
            sequence.Append(DOTween.To(() => this.transform.position.y, y =>
            {
                this.transform.position = new Vector3(this.transform.position.x, y);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
            }, worldEndPos.y, 0.6f).SetEase(Ease.InCubic));
            sequence.AppendCallback(() =>
            {
                if (hitEffect != null)
                {
                    hitEffect.SetActive(true);
                    icon.gameObject.transform.localScale = iconIdleScale;
                    itemCnt.gameObject.SetActive(false);

                }
                if (Bg != null) Bg.gameObject.SetActive(false);
                if (Frame != null) Frame.gameObject.SetActive(false);

                if(icon != null)
                    ProjectUtility.SetActiveCheck(icon.gameObject, false);


                SoundPlayer.Instance.PlaySound("get");

                CompositeDisposable disposables = new CompositeDisposable();
                var startcount = GameRoot.Instance.PlayTimeSystem.CreateCountDownObservable(1f);
                startcount.Subscribe(_ => {; }, () =>
                {
                    disposables.Clear();
                    if (!Addressables.ReleaseInstance(this.gameObject))
                        GameObject.Destroy(this.gameObject);
                }).AddTo(disposables);

                OnEnd?.Invoke();
            });

            var seq_2 = DOTween.Sequence();
            seq_2.AppendInterval(delay);
            seq_2.Append(DOTween.To(() => this.transform.position.x, x =>
            {
                this.transform.position = new Vector3(x, this.transform.position.y);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, 0f);
            }, worldEndPos.x, 1f).SetEase(Ease.InQuad));


            var seq_3 = DOTween.Sequence();
            seq_3.AppendInterval(delay);
            seq_3.Append(this.transform.DOScale(3.5f, 0.4f).SetEase(Ease.OutExpo));
            seq_3.Append(this.transform.DOScale(0.3f, 0.6f).SetEase(Ease.InExpo));
        }
        
    }

    IEnumerator WaitFrame()
    {
        yield return new WaitForEndOfFrame();
        icon.gameObject.SetActive(true);

        if(itemCnt != null)
        itemCnt.gameObject.SetActive(true);
    }
}
