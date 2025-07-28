using System.Collections.Generic;
using UnityEngine;

public class InGameScrollSea : MonoBehaviour
{
    [SerializeField]
    private Camera SubSeaCamera;

    [SerializeField]
    private FishingHookComponent HookCompoent;

    [SerializeField]
    private List<InGameSea> SeaList = new List<InGameSea>();

    public FishSpawner FishSpawner;

    private float CameraMinY = -28.4f;

    private Vector3 InitCamPos;

    private List<Vector3> InitMappos = new List<Vector3>();

    private int CurrentSeaIdx = 0;


    void Awake()
    {
        InitCamPos = SubSeaCamera.transform.position;

        foreach (var sea in SeaList)
        {
            InitMappos.Add(sea.transform.localPosition);
        }
    }

    public void Init()
    {
        HookCompoent.Init();

        foreach (var sea in SeaList)
        {
            sea.Set(1);
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

        // Sea 위치 재배치 로직 (완전히 새로 작성)
        RepositionSeaObjects(currentCameraY);

        // Hook이 너무 아래로 내려가면 위치만 리셋 (깊이 값은 유지)
        if (HookCompoent.FisshingHookTr.position.y < -100000f)
        {
            // Hook 위치만 리셋 (깊이는 유지)
            HookCompoent.ResetPositionKeepDepth();
            
            // 카메라도 초기 위치로
            SubSeaCamera.transform.position = InitCamPos;

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

    private void RepositionSeaObjects(float cameraY)
    {
        float sectionDistance = 27.21f;
        
        // 카메라 위치를 기준으로 필요한 Sea 위치들 계산
        List<float> requiredPositions = new List<float>();
        
        // 카메라 위쪽에서 아래쪽까지 Sea가 배치될 위치들 계산
        float baseY = CameraMinY; // 기준점
        
        // 카메라가 기준점보다 위에 있으면 기준점 위치부터 시작
        if (cameraY >= CameraMinY)
        {
            // 초기 위치들 사용
            for (int i = 0; i < SeaList.Count && i < InitMappos.Count; i++)
            {
                requiredPositions.Add(InitMappos[i].y);
            }
        }
        else
        {
            // 카메라가 아래에 있을 때 동적 계산
            float depthFromBase = CameraMinY - cameraY;
            int sectionsBelow = Mathf.FloorToInt(depthFromBase / sectionDistance);
            
            // 화면에 보여야 할 위치들 계산 (카메라 위아래로 여유있게)
            for (int i = 0; i < SeaList.Count; i++)
            {
                float targetY;
                
                if (i == 0)
                {
                    // 첫 번째 Sea는 항상 카메라 근처 또는 약간 위에
                    targetY = CameraMinY - sectionsBelow * sectionDistance;
                }
                else
                {
                    // 나머지 Sea들은 순차적으로 아래쪽에 배치
                    targetY = CameraMinY - (sectionsBelow + i) * sectionDistance;
                }
                
                requiredPositions.Add(targetY);
            }
        }
        
        // 계산된 위치에 Sea 객체들 배치
        for (int i = 0; i < SeaList.Count && i < requiredPositions.Count; i++)
        {
            Vector3 currentPos = SeaList[i].transform.localPosition;
            Vector3 targetPos = new Vector3(currentPos.x, requiredPositions[i], currentPos.z);
            
            SeaList[i].transform.localPosition = targetPos;
        }
        
        // 디버그 로그 - 특정 카메라 위치에서만
        if (cameraY <= -25f && cameraY >= -35f)
        {
            Debug.Log($"=== Sea Reposition Debug - CameraY: {cameraY:F2} ===");
            for (int i = 0; i < SeaList.Count; i++)
            {
                Debug.Log($"Sea{i}: Y={SeaList[i].transform.localPosition.y:F2} (Target: {(i < requiredPositions.Count ? requiredPositions[i].ToString("F2") : "N/A")})");
            }
        }
    }

}
