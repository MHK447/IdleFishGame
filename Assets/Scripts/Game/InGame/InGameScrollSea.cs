using System.Collections.Generic;
using UnityEngine;

public class InGameScrollSea : MonoBehaviour
{
    [SerializeField]
    private Camera SubSeaCamera;

    [SerializeField]
    private FishingHookComponent HookCompoent;

    private float CameraMinY = -28.4f;

    private float MaxCameraY = -70.1f;




    private Vector3 InitCamPos;

    private List<Vector3> InitMappos = new List<Vector3>();

    private float OffsetY = 5f;




    public float descendAmount = 2f;     // 얼마나 내릴지
    public float moveSpeed = 5f;         // 얼마나 부드럽게 이동할지


    //private Vector2[] SeaVecPoss =  {new Vector2(-100f , -100f), new}

    private bool isMoving = false;


    [SerializeField]
    private List<Transform> SeaTrList = new List<Transform>();

    private int rowcount = 0;

    private float previousCameraY = 0f; // 이전 프레임의 카메라 Y 위치를 저장할 변수

    void Awake()
    {
        InitCamPos = SubSeaCamera.transform.position;

        foreach (Transform sea in SeaTrList)
        {
            InitMappos.Add(sea.localPosition);
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

        // 카메라 이동 방향 감지
        bool movingDown = currentCameraY < previousCameraY;
        bool movingUp = currentCameraY > previousCameraY;

        // 아래로 내려갈 때 (경계를 넘었고 아래로 이동 중일 때)
        if (currentCameraY <= MaxCameraY && movingDown)
        {
            MaxCameraY -= 27.21f;

            SeaTrList[rowcount].transform.localPosition = new Vector3(
                SeaTrList[rowcount].transform.localPosition.x,
                MaxCameraY - 5f,
                SeaTrList[rowcount].transform.localPosition.z
            );

            rowcount += 1;
            if (rowcount >= SeaTrList.Count)
                rowcount = 0;
        }
        // 위로 올라갈 때 (경계를 넘었고 위로 이동 중일 때)
        else if (currentCameraY >= MaxCameraY && movingUp)
        {
            MaxCameraY += 27.21f;

            // 현재 rowcount가 가리키는 오브젝트를 위쪽에 재배치
            SeaTrList[rowcount].transform.localPosition = new Vector3(
                SeaTrList[rowcount].transform.localPosition.x,
                MaxCameraY + 5f,  // 새로운 경계보다 조금 위에 배치 (아래로 갈 때와 대칭)
                SeaTrList[rowcount].transform.localPosition.z
            );

            // 다음 사이클을 위해 rowcount 조정
            rowcount -= 1;
            if (rowcount < 0)
                rowcount = SeaTrList.Count - 1;
        }

        // 이전 프레임 위치 저장
        previousCameraY = currentCameraY;

        // 너무 아래로 내려가면 리셋
        if (currentCameraY < -5000f)
        {
            SubSeaCamera.transform.position = InitCamPos;
            HookCompoent.Init();
            rowcount = 0;
            MaxCameraY = -70.1f;
            CameraMinY = -28.4f;
            previousCameraY = InitCamPos.y;

            for (int i = 0; i < SeaTrList.Count; i++)
            {
                SeaTrList[i].transform.localPosition = InitMappos[i];
            }
        }
    }


}
