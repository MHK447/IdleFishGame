using UnityEngine;
using DG.Tweening;
using System.Collections;
using BanpoFri;
using UniRx;

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

    [SerializeField]
    private int lineSegments = 20; // 라인 세그먼트 수

    [SerializeField]
    private int minLineSegments = 15; // 최소 라인 세그먼트 수 (고속 이동시)

    [SerializeField]
    private float speedThreshold = 15f; // 세그먼트 감소 속도 임계값

    [SerializeField]
    private float lineSag = 0.5f; // 라인 처짐 정도

    [SerializeField]
    private float lineUpdateThreshold = 0.1f; // 라인 업데이트 최소 거리

    [SerializeField]
    private int maxLineUpdatesPerSecond = 60; // 초당 최대 라인 업데이트 횟수

    [SerializeField]
    private bool ultraFastLineUpdate = true; // 초고속 라인 업데이트 모드

    private float lastLineUpdateTime = 0f;
    private Vector3 lastHookPosition;

    [SerializeField]
    private float hookDownSpeed = 2f;  // 내려가는 속도 (유닛/초)

    [SerializeField]
    private float hookUpSpeed = 5f;    // 올라가는 속도 (유닛/초)

    private Vector3 StartPos;
    private Vector3 HookStartPos; // FisshingHookObj의 초기 위치 저장

    private float accumulatedDepth = 0f;  // 누적 수심(m 단위)
    private float lastY;  // 이전 프레임의 y값 저장

    public float metersPerUnit = 0.01f;  // 10 유닛 = 1m → 1 유닛 = 0.1m

    private float StartHookY = 0f;
    private float targetY = 0f;  // 목표 Y 위치

    private float NoneAutoDowndeltime = 0f;

    private FishingHookState CurHookState = FishingHookState.None;

    private InGameScrollSea ScrollSea;

    private int CautchFishIdx = 0;

    private InGameFish CautchFish = null;

    private float HookTouchSpeed = 0f;




    void Awake()
    {
        StartPos = transform.position;
        HookStartPos = FisshingHookObj.position; // Hook 오브젝트의 초기 위치 저장

        accumulatedDepth = 0f;
        lastY = FisshingHookObj.position.y;
        lastHookPosition = FisshingHookObj.position; // 라인 업데이트용 초기 위치

        LineRenderer.positionCount = lineSegments + 1;
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


    void FixedUpdate()
    {
        float currentY = FisshingHookObj.position.y;
        float deltaY = lastY - currentY;

        // y가 감소했을 때만 누적
        accumulatedDepth += deltaY * (1f / metersPerUnit);

        lastY = currentY;

        GameRoot.Instance.PlayerSystem.SeaDepthProperty.Value = accumulatedDepth / 10f;

        // 훅 이동 처리
        UpdateHookMovement();

        UpdateHookTouchSpeed();

        // 낚시줄 길이 조정 - 조건부 업데이트
        UpdateLineLength();
    }

    private void UpdateLineLength()
    {
        Vector3 currentHookPos = FisshingHookObj.position;

        // 초고속 모드일 때는 업데이트 제한을 거의 없앰
        if (ultraFastLineUpdate)
        {
            // 매 프레임 업데이트 (성능이 허용하는 한)
            // 속도에 관계없이 최대 세그먼트 사용
            int ultraCurrentSegments = lineSegments;

            // 라인 렌더러 포인트 수 조정
            if (LineRenderer.positionCount != ultraCurrentSegments + 1)
            {
                LineRenderer.positionCount = ultraCurrentSegments + 1;
            }

            Vector3 ultraStartPos = LinrStartTr.position;
            Vector3 ultraEndPos = LineEndTr.position;

            // 시작점과 끝점 사이의 거리 계산
            float ultraDistance = Vector3.Distance(ultraStartPos, ultraEndPos);

            // 라인의 처짐 정도를 거리에 비례하여 계산 (최대값 제한)
            float ultraCurrentSag = Mathf.Min(lineSag * (ultraDistance / 10f), lineSag * 2f);

            // 라인의 각 점을 계산
            for (int i = 0; i <= ultraCurrentSegments; i++)
            {
                float t = (float)i / ultraCurrentSegments; // 0부터 1까지의 비율

                // 기본 선형 보간
                Vector3 targetPos = Vector3.Lerp(ultraStartPos, ultraEndPos, t);

                // 포물선 형태의 처짐 추가 (중간에서 가장 많이 처짐)
                float sagAmount = ultraCurrentSag * Mathf.Sin(t * Mathf.PI);
                targetPos.y -= sagAmount;

                // 바로 적용 (부드러운 보간 없음)
                LineRenderer.SetPosition(i, targetPos);
            }
            return;
        }

        // 기존 방식 (제한된 업데이트)
        float currentTime = Time.time;

        // 업데이트 조건을 훨씬 빠르게 설정
        float timeSinceLastUpdate = currentTime - lastLineUpdateTime;
        float distanceMoved = Vector3.Distance(currentHookPos, lastHookPosition);
        float minUpdateInterval = 1f / 120f; // 120fps로 증가 (기존 60fps에서)

        // 거리 임계값을 낮춰서 더 자주 업데이트
        float dynamicLineUpdateThreshold = lineUpdateThreshold * 0.1f; // 기존의 10%로 감소

        // 업데이트 조건을 더 관대하게 설정
        bool shouldUpdate = timeSinceLastUpdate >= minUpdateInterval ||
                           distanceMoved >= dynamicLineUpdateThreshold ||
                           timeSinceLastUpdate >= 0.008f; // 강제 업데이트 (약 125fps)

        if (!shouldUpdate) return;

        // 속도 계산 (0으로 나누는 것 방지)
        float speed = timeSinceLastUpdate > 0 ? distanceMoved / timeSinceLastUpdate : 0f;

        // 속도에 따른 세그먼트 수 조절 (빠른 속도에서도 더 많은 세그먼트 유지)
        int currentSegments = speed > speedThreshold ?
            Mathf.Max(minLineSegments, lineSegments - 3) : lineSegments;

        // 라인 렌더러 포인트 수 조정
        if (LineRenderer.positionCount != currentSegments + 1)
        {
            LineRenderer.positionCount = currentSegments + 1;
        }

        // 업데이트 시간과 위치 기록
        lastLineUpdateTime = currentTime;
        lastHookPosition = currentHookPos;

        Vector3 startPos = LinrStartTr.position;
        Vector3 endPos = LineEndTr.position;

        // 시작점과 끝점 사이의 거리 계산
        float distance = Vector3.Distance(startPos, endPos);

        // 라인의 처짐 정도를 거리에 비례하여 계산 (최대값 제한)
        float currentSag = Mathf.Min(lineSag * (distance / 10f), lineSag * 2f);

        // 라인의 각 점을 계산
        for (int i = 0; i <= currentSegments; i++)
        {
            float t = (float)i / currentSegments; // 0부터 1까지의 비율

            // 기본 선형 보간
            Vector3 targetPos = Vector3.Lerp(startPos, endPos, t);

            // 포물선 형태의 처짐 추가 (중간에서 가장 많이 처짐)
            float sagAmount = currentSag * Mathf.Sin(t * Mathf.PI);
            targetPos.y -= sagAmount;

            // 바로 적용 (부드러운 보간 없음)
            LineRenderer.SetPosition(i, targetPos);
        }
    }

    private void UpdateHookMovement()
    {
        Vector3 currentPos = FisshingHookObj.position;

        switch (CurHookState)
        {
            case FishingHookState.HookDown:
                HookDownCheck();
                break;

            case FishingHookState.HookUp:
                if (currentPos.y < targetY)
                {
                    var buffvalue = GameRoot.Instance.UpgradeSystem.GetUpgradeValue(UpgradeSystem.UpgradeType.FisihngSpeeed);
                    float newY = currentPos.y + (hookUpSpeed + buffvalue + HookTouchSpeed) * Time.deltaTime;
                    newY = Mathf.Min(newY, targetY); // 목표점을 넘지 않도록
                    FisshingHookObj.position = new Vector3(currentPos.x, newY, currentPos.z);

                    // 목표 지점 도달 시
                    if (newY >= targetY)
                    {


                        //위에로프도 위로 하기 
                        TopRopeTr.DOScaleY(0, 0.2f).OnComplete(() =>
                        {
                            CautchFishAction(CautchFishIdx);
                            StartHookDown();
                        });
                    }
                }
                break;
        }
    }


    public void StartHookDown()
    {
        TopRopeTr.DOScaleY(22f, 0.2f).OnComplete(() =>
                        {
                            GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.HookDown, 0.5f));
                        });
    }


    public void HookDownCheck()
    {
        Vector3 currentPos = FisshingHookObj.position;
        if (GameRoot.Instance.UserData.Fishingautoproperty.Value)
        {
            if (currentPos.y > targetY)
            {
                var buffvalue = GameRoot.Instance.UpgradeSystem.GetUpgradeValue(UpgradeSystem.UpgradeType.FisihngSpeeed);
                float newY = currentPos.y - (hookDownSpeed + buffvalue + HookTouchSpeed) * Time.deltaTime;
                newY = Mathf.Max(newY, targetY); // 목표점을 넘지 않도록
                FisshingHookObj.position = new Vector3(currentPos.x, newY, currentPos.z);

                // 목표 지점 도달 시
                if (newY <= targetY)
                {
                    GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.FishingIdle));
                }
            }
        }
        else
        {
            if (FisshingHookObj.position.y <= -10f)
            {
                NoneAutoDowndeltime += Time.deltaTime;
                if (NoneAutoDowndeltime >= 2f)
                {
                    NoneAutoDowndeltime = 0f;
                    GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.FishingIdle));
                }
            }

            if (Input.GetMouseButton(0))
            {
                NoneAutoDowndeltime = 0f;
                float newY = currentPos.y - 2f * Time.deltaTime;
                newY = Mathf.Max(newY, targetY); // 목표점을 넘지 않도록
                FisshingHookObj.position = new Vector3(currentPos.x, newY, currentPos.z);

                if (newY <= targetY)
                {
                    GameRoot.Instance.StartCoroutine(ChangeHookState(FishingHookState.FishingIdle));
                }
            }
        }
    }


    public void HookDown()
    {
        var depthvalue = GameRoot.Instance.UpgradeSystem.GetUpgradeValue(UpgradeSystem.UpgradeType.WaterDepth);
        targetY = StartHookY - depthvalue;
    }

    public void FishingIdle()
    {
        GameRoot.Instance.WaitTimeAndCallback(5f, () =>
        {

            CautchFish = ScrollSea.RandCatchFish(
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
            System.Numerics.BigInteger moneyvalue = fishinfotd.money_value * (long)GameRoot.Instance.UpgradeSystem.GetUpgradeValue(UpgradeSystem.UpgradeType.PriceMulti);

            GameRoot.Instance.EffectSystem.MultiPlay<TextEffectMoney>(TopRopeTr.position, x =>
            {
                x.SetText(moneyvalue);
                x.SetAutoRemove(true, 1.5f);
            });

            CautchFish.ReturnSpawner();



        }

    }


    public void HookUp()
    {
        targetY = StartHookY;
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
                HookTouchSpeed = 0;
                HookDown();
                break;
            case FishingHookState.HookUp:
                HookTouchSpeed = 0;
                HookUp();
                break;
            case FishingHookState.FishingIdle:
                FishingIdle();
                break;
        }
    }



    public void UpdateHookTouchSpeed()
    {

        if (Input.GetMouseButtonDown(0) && (CurHookState == FishingHookState.HookDown || CurHookState == FishingHookState.HookUp))
        {
            HookTouchSpeed += 3f;
        }
    }
}
