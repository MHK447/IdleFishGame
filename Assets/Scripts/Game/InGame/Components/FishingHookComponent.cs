using UnityEngine;
using DG.Tweening;
using System.Collections;
using BanpoFri;

public class FishingHookComponent : MonoBehaviour
{
    public enum FishingHookState
    {
        None,
        HookDown,
        HookUp,
        FishingIdle,
    }


    [SerializeField]
    private Transform FisshingHookObj;

    [SerializeField]
    private Transform TopRopeTr;

    public Transform FisshingHookTr { get { return FisshingHookObj; } }

    [SerializeField]
    private LineRenderer LineRenderer;

    [SerializeField]
    private Transform LinrStartTr;

    [SerializeField]
    private Transform LineEndTr;

    private Vector3 StartPos;
    private Vector3 HookStartPos; // FisshingHookObj의 초기 위치 저장

    private float accumulatedDepth = 0f;  // 누적 수심(m 단위)
    private float lastY;  // 이전 프레임의 y값 저장

    public float metersPerUnit = 0.01f;  // 10 유닛 = 1m → 1 유닛 = 0.1m

    private float StartHookY = 0f;

    private FishingHookState CurHookState = FishingHookState.None;

    private InGameScrollSea ScrollSea;

    private int CautchFishIdx = 0;

    void Awake()
    {
        StartPos = transform.position;
        HookStartPos = FisshingHookObj.position; // Hook 오브젝트의 초기 위치 저장

        accumulatedDepth = 0f;
        lastY = FisshingHookObj.position.y;

        LineRenderer.positionCount = 2;
        LineRenderer.startWidth = 0.15f;
        LineRenderer.endWidth = 0.15f;

        StartHookY = FisshingHookObj.position.y;


        CurHookState = FishingHookState.None;

    }


    public void Init()
    {
        this.transform.position = StartPos;
        FisshingHookObj.position = HookStartPos; // Hook 오브젝트도 초기 위치로 리셋

        ScrollSea = GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().GetScrollSea;

        GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.HookDown));
    }

    // 깊이는 유지하면서 위치만 리셋하는 메서드
    public void ResetPositionKeepDepth()
    {
        this.transform.position = StartPos;
        FisshingHookObj.position = HookStartPos; // Hook 오브젝트도 초기 위치로 리셋
        lastY = FisshingHookObj.position.y; // lastY를 리셋된 위치로 업데이트
    }


    void Update()
    {
        float currentY = FisshingHookObj.position.y;
        float deltaY = lastY - currentY;

        // y가 감소했을 때만 누적
        accumulatedDepth += deltaY * (1f / metersPerUnit);


        lastY = currentY;

        GameRoot.Instance.PlayerSystem.SeaDepthProperty.Value = accumulatedDepth / 10f;

        // 낚시줄 길이 조정
        UpdateLineLength();
    }

    private void UpdateLineLength()
    {
        LineRenderer.SetPosition(0, LinrStartTr.position);
        LineRenderer.SetPosition(1, LineEndTr.position);
    }



    public void HookDown()
    {
        var depthvalue = GameRoot.Instance.UpgradeSystem.GetUpgradeValue(UpgradeSystem.UpgradeType.WaterDepth);

        FisshingHookObj.transform.DOMoveY(StartHookY - depthvalue, 2f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.FishingIdle));
        });
    }

    public void FishingIdle()
    {
        GameRoot.Instance.WaitTimeAndCallback(5f, () =>
        {

            ScrollSea.RandCatchFish(
                (fishidx) =>
                {

                    CautchFishIdx = fishidx;

                    GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.HookUp));
                }
            );
        });
    }


    public void CautchFishAction(int fishidx)
    {
        var fishinfotd = Tables.Instance.GetTable<FishInfo>().GetData(fishidx);

        if (fishinfotd != null)
        {
            GameRoot.Instance.EffectSystem.MultiPlay<TextEffectMoney>(TopRopeTr.position, x =>
            {
                x.SetText(fishinfotd.money_value);
                x.SetAutoRemove(true, 1.5f);
            });
        }

    }


    public void HookUp()
    {
        FisshingHookObj.transform.DOMoveY(StartHookY, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            CautchFishAction(CautchFishIdx);
            GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.HookDown, 0.5f));
        });
    }


    public IEnumerator ChangeHookState(FishingHookState state, float waittime = 0f)
    {
        yield return new WaitForSeconds(waittime);

        if (CurHookState == state)
        {
            yield break;
        }

        CurHookState = state;

        switch (state)
        {
            case FishingHookState.HookDown:
                HookDown();
                break;
            case FishingHookState.HookUp:
                HookUp();
                break;
            case FishingHookState.FishingIdle:
                FishingIdle();
                break;
        }
    }
}
