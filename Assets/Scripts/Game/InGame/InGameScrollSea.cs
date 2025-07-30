using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InGameScrollSea : MonoBehaviour
{
    [SerializeField]
    private Camera SubSeaCamera;

    [SerializeField]
    private FishingHookComponent HookCompoent;

    [SerializeField]
    private List<InGameSea> SeaList = new List<InGameSea>();

    private float CameraMinY = -28.4f;

    private Vector3 InitCamPos;

    private List<Vector3> InitMappos = new List<Vector3>();

    private int CurrentSeaIdx = 0;

    // 무한 스크롤링을 위한 새 변수들
    private float prevCameraY;
    private float sectionDistance = 27.21f;
    private float cameraViewHeight = 20f; // 카메라가 보는 영역 높이
    private int currentDepthLevel = 1; // 현재 깊이 레벨


    void Awake()
    {
        InitCamPos = SubSeaCamera.transform.position;
        prevCameraY = InitCamPos.y;

        foreach (var sea in SeaList)
        {
            InitMappos.Add(sea.transform.localPosition);
        }
    }

    public void Init()
    {
        HookCompoent.Init();
        currentDepthLevel = 0;
        prevCameraY = InitCamPos.y;

        // 초기 바다들을 깊이에 맞게 설정
        for (int i = 0; i < SeaList.Count; i++)
        {
            currentDepthLevel++;
            SeaList[i].Init();
            SeaList[i].Set(i + 1);
        }
    }

    void Update()
    {
        if (HookCompoent == null) return;


        float camy = HookCompoent.FisshingHookTr.position.y > CameraMinY ? CameraMinY : HookCompoent.FisshingHookTr.position.y;

        SubSeaCamera.transform.position = new Vector3(
            SubSeaCamera.transform.position.x,
            camy,
            SubSeaCamera.transform.position.z
        );

        float currentCameraY = SubSeaCamera.transform.position.y;

        // 무한 스크롤링 로직
        HandleInfiniteScrolling(currentCameraY);

        // 이전 카메라 위치 업데이트
        prevCameraY = currentCameraY;

        // Hook이 너무 아래로 내려가면 위치만 리셋 (깊이 값은 유지)
        if (HookCompoent.FisshingHookTr.position.y < -100000f)
        {
            // Hook 위치만 리셋 (깊이는 유지)
            HookCompoent.ResetPositionKeepDepth();
            
            // 카메라도 초기 위치로
            SubSeaCamera.transform.position = InitCamPos;
            prevCameraY = InitCamPos.y;

            // Sea 오브젝트들의 위치만 초기화
            for (int i = 0; i < SeaList.Count; i++)
            {
                SeaList[i].transform.localPosition = InitMappos[i];
            }
        }
    }


    public InGameFish RandCatchFish(System.Action<int> cauthaction)
    {
        if (HookCompoent == null || HookCompoent.FisshingHookTr == null || SeaList.Count == 0)
            return null;

        Vector3 hookPosition = HookCompoent.FisshingHookTr.position;
        
        // 낚싯바늘에 가장 가까운 바다 찾기
        InGameSea closestSea = null;
        float closestDistance = float.MaxValue;
        
        foreach (var sea in SeaList)
        {
            float distance = sea.GetDistanceToPoint(hookPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSea = sea;
            }
        }
        
        // 가장 가까운 바다에서 랜덤한 물고기 선택
        if (closestSea != null)
        {
            InGameFish targetFish = closestSea.RandCatchFish();
            
            if (targetFish != null)
            {
                // 선택된 물고기가 낚싯바늘을 타겟으로 설정하고 콜백 등록
                targetFish.SetTarget(HookCompoent.FisshingHookTr, cauthaction);
                return targetFish;
            }
        }
        
        return null;
    }

    private void HandleInfiniteScrolling(float currentCameraY)
    {
        float cameraMovement = currentCameraY - prevCameraY;
        
        // 카메라가 아래로 이동 (더 깊이 들어감) - 더 민감하게 반응
        if (cameraMovement < -0.5f)
        {
            RepositionFarthestSeaForDownward(currentCameraY);
        }
        // 카메라가 위로 이동 (덜 깊어짐) - 더 민감하게 반응
        else if (cameraMovement > 0.5f)
        {
            RepositionFarthestSeaForUpward(currentCameraY);
        }
    }

    private void RepositionFarthestSeaForDownward(float cameraY)
    {
        // 카메라 위치에서 가장 먼 바다 찾기 (위쪽에 있는 바다)
        InGameSea farthestSea = null;
        float maxDistance = 0f;
        
        foreach (var sea in SeaList)
        {
            float distance = sea.transform.localPosition.y - cameraY;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestSea = sea;
            }
        }
        
        // y포지션이 40 정도 차이가 나면 재배치
        if (farthestSea != null && maxDistance > 40f)
        {
            // 가장 아래에 있는 바다 찾기
            InGameSea bottomSea = SeaList.OrderBy(s => s.transform.localPosition.y).First();
            float newY = bottomSea.transform.localPosition.y - sectionDistance;
            
            // 바다를 새 위치로 이동
            Vector3 currentPos = farthestSea.transform.localPosition;
            farthestSea.transform.localPosition = new Vector3(currentPos.x, newY, currentPos.z);
            
            // 깊이 레벨 계산 및 바다 정보 업데이트
            currentDepthLevel++;
            int newDepthLevel = currentDepthLevel + SeaList.Count - 1;
            farthestSea.Set(currentDepthLevel);
        }
    }

    private void RepositionFarthestSeaForUpward(float cameraY)
    {
        // 카메라 위치에서 가장 먼 바다 찾기 (아래쪽에 있는 바다)
        InGameSea farthestSea = null;
        float maxDistance = 0f;
        
        foreach (var sea in SeaList)
        {
            float distance = cameraY - sea.transform.localPosition.y;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestSea = sea;
            }
        }
        
        // y포지션이 40 정도 차이가 나면 재배치
        if (farthestSea != null && maxDistance > 40f && currentDepthLevel > 4)
        {
            // 가장 위에 있는 바다 찾기
            InGameSea topSea = SeaList.OrderByDescending(s => s.transform.localPosition.y).First();


            InGameSea DownSea = SeaList.OrderByDescending(s => s.transform.localPosition.y).First();
            float newY = topSea.transform.localPosition.y + sectionDistance;
            
            // 바다를 새 위치로 이동
            Vector3 currentPos = farthestSea.transform.localPosition;
            farthestSea.transform.localPosition = new Vector3(currentPos.x, newY, currentPos.z);
            
            // 깊이 레벨 계산 및 바다 정보 업데이트
            currentDepthLevel--;
            farthestSea.Set(DownSea.GetSeaidx - SeaList.Count - 1);
        }
    }

}

