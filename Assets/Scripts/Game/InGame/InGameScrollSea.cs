using System.Collections.Generic;
using UnityEngine;

public class InGameScrollSea : MonoBehaviour
{
    [SerializeField]
    private Camera SubSeaCamera;

    private float CameraMinY = -108;

    private float MaxCameraY = -139.2f;


    private Vector3 InitCamPos;

    private List<Vector3> InitMappos = new List<Vector3>();




    public float descendAmount = 2f;     // 얼마나 내릴지
    public float moveSpeed = 5f;         // 얼마나 부드럽게 이동할지


   //private Vector2[] SeaVecPoss =  {new Vector2(-100f , -100f), new}

    private bool isMoving = false;


    [SerializeField]
    private List<Transform> SeaTrList = new List<Transform>();

    private int rowcount = 0;

    void Awake()
    {
        InitCamPos = SubSeaCamera.transform.position;

        foreach(Transform sea in SeaTrList)
        {
            InitMappos.Add(sea.localPosition);
        }
    }

    void Update()
    {
        // 마우스 왼쪽 클릭 시
        // if (Input.GetMouseButtonDown(0))
        // {
        //     SubSeaCamera.transform.position = new Vector3(SubSeaCamera.transform.position.x, SubSeaCamera.transform.position.y - descendAmount, SubSeaCamera.transform.position.z);
        //     isMoving = true;
        // }

        // if (Input.GetMouseButtonUp(0))
        // {
        //     isMoving = false;
        // }

        if (isMoving)
        {
            // SubSeaCamera.transform.position =
            //  Vector3.Lerp(SubSeaCamera.transform.position, new Vector3(SubSeaCamera.transform.position.x, SubSeaCamera.transform.position.y - descendAmount, SubSeaCamera.transform.position.z), Time.deltaTime * moveSpeed);


            if(SubSeaCamera.transform.position.y <= MaxCameraY)
            {
                MaxCameraY -= 19.6f;
                SeaTrList[rowcount].transform.localPosition = new Vector3(SeaTrList[rowcount].transform.localPosition.x, 
                MaxCameraY, SeaTrList[rowcount].transform.localPosition.z);

                rowcount += 1;

                if(rowcount >= SeaTrList.Count)
                {
                    rowcount = 0;
                }
            }
        }



        if(SubSeaCamera.transform.position.y < -5000f)
        {
            SubSeaCamera.transform.position = InitCamPos;

            rowcount = 0;

            MaxCameraY = -139.2f;

            for(int i = 0 ; i < SeaTrList.Count ; i++)
            {
                SeaTrList[i].transform.localPosition = InitMappos[i];
            }
        }
    }


}
