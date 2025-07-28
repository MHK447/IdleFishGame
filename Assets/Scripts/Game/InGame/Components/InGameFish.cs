using UnityEngine;
using BanpoFri;
using UnityEngine.AddressableAssets;

public class InGameFish : MonoBehaviour
{
    private int FishIdx = 0;

    public int GetFishIdx { get { return FishIdx; } }

    private InGameFishBody FishBody;

    [Header("Fish Movement Settings")]
    [SerializeField] private Vector2 defaultMovementBounds = new Vector2(8f, 4f);
    [SerializeField] private float defaultMoveSpeed = 2f;

    public void Set(int fishidx)
    {
        if (FishBody != null && FishIdx != fishidx)
        {
            Destroy(FishBody.gameObject);
            FishBody = null;
        }

        FishIdx = fishidx;

        Addressables.InstantiateAsync($"Fish_{fishidx}").Completed += (handle) =>
        {
            FishBody = handle.Result.GetComponent<InGameFishBody>();
            FishBody.transform.SetParent(this.transform, false);
            
            // 물고기 종류에 따른 움직임 설정
            SetupFishMovement();
        };
    }

    private void SetupFishMovement()
    {
        if (FishBody != null)
        {
            // 물고기 테이블 데이터에서 설정값 가져오기
            var fishData = Tables.Instance.GetTable<FishInfo>().GetData(FishIdx);
            if (fishData != null)
            {
                // 물고기 크기/종류에 따라 속도와 움직임 범위 조정
                float speedMultiplier = GetSpeedMultiplier(fishData);
                Vector2 movementBounds = GetMovementBounds(fishData);
                
                FishBody.SetMoveSpeed(defaultMoveSpeed * speedMultiplier);
                FishBody.SetMovementBounds(movementBounds);
            }
            else
            {
                // 기본값 설정
                FishBody.SetMoveSpeed(defaultMoveSpeed);
                FishBody.SetMovementBounds(defaultMovementBounds);
            }
            
            // 움직임 시작
            FishBody.StartMovement();
        }
    }

    private float GetSpeedMultiplier(FishInfoData fishData)
    {
        // 물고기 인덱스나 다른 속성에 따라 속도 배수 결정
        // 작은 물고기는 빠르게, 큰 물고기는 천천히
        switch (FishIdx)
        {
            case 1: return 1.2f; // 작은 물고기
            case 2: return 1.0f; // 보통 물고기
            case 3: return 0.8f; // 큰 물고기
            case 4: return 0.6f; // 매우 큰 물고기
            default: return 1.0f;
        }
    }

    private Vector2 GetMovementBounds(FishInfoData fishData)
    {
        // 물고기 종류에 따라 움직임 범위 조정
        switch (FishIdx)
        {
            case 1: return new Vector2(6f, 3f);   // 작은 범위
            case 2: return new Vector2(8f, 4f);   // 보통 범위
            case 3: return new Vector2(10f, 5f);  // 큰 범위
            case 4: return new Vector2(12f, 6f);  // 매우 큰 범위
            default: return defaultMovementBounds;
        }
    }

    public void ReturnSpawner()
    {
        // 움직임 정지 후 스포너로 반환
        if (FishBody != null)
        {
            FishBody.StopMovement();
        }
        
        ProjectUtility.SetActiveCheck(this.gameObject, false);
    }

    // 외부에서 움직임 제어할 수 있는 메서드들
    public void StartMovement()
    {
        if (FishBody != null)
            FishBody.StartMovement();
    }

    public void StopMovement()
    {
        if (FishBody != null)
            FishBody.StopMovement();
    }

    public void SetCustomMovementSettings(float speed, Vector2 bounds)
    {
        if (FishBody != null)
        {
            FishBody.SetMoveSpeed(speed);
            FishBody.SetMovementBounds(bounds);
        }
    }
}
