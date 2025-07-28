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

    private int currentSection = 0;        // 현재 카메라가 있는 구간
    private int previousSection = 0;       // 이전 프레임의 구간

    private Vector3 InitCamPos;

    private List<Vector3> InitMappos = new List<Vector3>();

    public float descendAmount = 2f;     // 얼마나 내릴지
    public float moveSpeed = 5f;         // 얼마나 부드럽게 이동할지

    private int rowcount = 0;

    private int CurrentSeaIdx = 0;


    void Awake()
    {
        InitCamPos = SubSeaCamera.transform.position;

        foreach (var sea in SeaList)
        {
            InitMappos.Add(sea.transform.localPosition);
        }
        
        // 초기 구간을 실제 카메라 위치를 기준으로 계산
        float initialCameraY = SubSeaCamera.transform.position.y;
        if (initialCameraY >= CameraMinY)
        {
            currentSection = 0;
        }
        else
        {
            currentSection = Mathf.FloorToInt((CameraMinY - initialCameraY) / 27.21f) + 1;
        }
        
        previousSection = currentSection;
        
        // rowcount도 초기화
        rowcount = 0;
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

        // 현재 카메라가 어떤 구간에 있는지 계산
        if (currentCameraY >= CameraMinY)
        {
            currentSection = 0;
        }
        else
        {
            currentSection = Mathf.FloorToInt((CameraMinY - currentCameraY) / 27.21f) + 1;
        }

        // 구간이 바뀌었을 때만 재배치
        if (currentSection != previousSection)
        {
            // 아래로 내려갔을 때 (구간 번호 증가)
            if (currentSection > previousSection)
            {
                // 현재 보이는 가장 위쪽 오브젝트를 아래쪽으로 이동
                float newY = CameraMinY - (currentSection + 1) * 27.21f;
                
                SeaList[rowcount].transform.localPosition = new Vector3(
                    SeaList[rowcount].transform.localPosition.x,
                    newY,
                    SeaList[rowcount].transform.localPosition.z 
                );

                rowcount = (rowcount + 1) % SeaList.Count;
            }
            // 위로 올라갔을 때 (구간 번호 감소)
            else if (currentSection < previousSection)
            {
                // rowcount를 먼저 조정
                rowcount = (rowcount - 1 + SeaList.Count) % SeaList.Count;
                
                // 현재 구간 위쪽에 배치 (더 안전한 계산)
                float newY = CameraMinY - Mathf.Max(0, currentSection - 1) * 27.21f;
                
                SeaList[rowcount].transform.localPosition = new Vector3(
                    SeaList[rowcount].transform.localPosition.x,
                    newY,
                    SeaList[rowcount].transform.localPosition.z
                );
            }
            
            previousSection = currentSection;
        }

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
            
            // 현재 섹션 정보만 초기화
            currentSection = 0;
            previousSection = 0;
        }
    }


    // public InGameFish RandCatchFish()
    // {
    //     //return SeaList[rowcount].RandCatchFish();
    // }

}
