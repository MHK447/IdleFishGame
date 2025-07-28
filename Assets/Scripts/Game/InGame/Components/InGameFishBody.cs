using UnityEngine;

public enum FishMovementMode
{
    Random,      // 랜덤 움직임
    FollowTarget, // 타겟 추적
    Caught       // 낚싯바늘을 물어서 정지
}

public class InGameFishBody : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector2 movementBounds = new Vector2(8f, 4f); // 움직임 영역 범위 (가로, 세로)
    
    [Header("Behavior Settings")]
    [SerializeField] private float changeDirectionInterval = 2f; // 방향 바꾸는 간격
    [SerializeField] private float detectionDistance = 1f; // 경계 감지 거리
    [SerializeField] private float rotationSpeed = 3f; // 회전 속도
    [SerializeField] private float maxRotationAngle = 45f; // 최대 회전 각도 제한
    
    [Header("Target Following Settings")]
    [SerializeField] private float targetFollowSpeed = 3f; // 타겟 추적 속도
    [SerializeField] private float catchDistance = 0.5f; // 낚싯바늘을 무는 거리
    [SerializeField] private float targetDetectionRange = 5f; // 타겟 감지 범위
    
    private Vector3 targetDirection;
    private Vector3 initialPosition;
    private float changeDirectionTimer;
    private bool isMoving = false;
    private bool facingRight = true; // 물고기가 오른쪽을 보고 있는지
    
    // 타겟 추적 관련
    private FishMovementMode movementMode = FishMovementMode.Random;
    private Transform currentTarget;
    private Vector3 lastTargetPosition;
    
    void Start()
    {
        InitializeFish();
    }
    
    void Update()
    {
        if (isMoving)
        {
            switch (movementMode)
            {
                case FishMovementMode.Random:
                    HandleRandomMovement();
                    break;
                case FishMovementMode.FollowTarget:
                    HandleTargetFollowing();
                    break;
                case FishMovementMode.Caught:
                    HandleCaughtState();
                    break;
            }
            
            Handle2DRotation();
        }
    }
    
    private void InitializeFish()
    {
        initialPosition = transform.position;
        SetRandomDirection();
        changeDirectionTimer = changeDirectionInterval;
        isMoving = true;
        movementMode = FishMovementMode.Random;
    }
    
    private void HandleRandomMovement()
    {
        HandleMovement();
        HandleDirectionChange();
        CheckBoundaries();
    }
    
    private void HandleTargetFollowing()
    {
        if (currentTarget == null)
        {
            // 타겟이 없으면 랜덤 움직임으로 복귀
            SetMovementMode(FishMovementMode.Random);
            return;
        }
        
        Vector3 targetPos = currentTarget.position;
        Vector3 currentPos = transform.position;
        float distanceToTarget = Vector3.Distance(currentPos, targetPos);
        
        // 낚싯바늘을 물 정도로 가까워지면 정지
        if (distanceToTarget <= catchDistance)
        {
            CatchTarget();
            return;
        }
        
        // 타겟 방향으로 이동
        Vector3 directionToTarget = (targetPos - currentPos).normalized;
        targetDirection = directionToTarget;
        
        // 타겟 추적 속도로 이동
        transform.position += targetDirection * targetFollowSpeed * Time.deltaTime;
        
        lastTargetPosition = targetPos;
    }
    
    private void HandleCaughtState()
    {
        // 낚싯바늘을 물었을 때는 타겟을 따라다님 (가만히 있지 않고 끌려다님)
        if (currentTarget != null)
        {
            Vector3 targetPos = currentTarget.position;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
            
            // 타겟 방향 설정 (회전용)
            Vector3 directionToTarget = (targetPos - lastTargetPosition).normalized;
            if (directionToTarget.magnitude > 0.1f)
            {
                targetDirection = directionToTarget;
            }
            
            lastTargetPosition = targetPos;
        }
    }
    
    private void HandleMovement()
    {
        // 현재 방향으로 이동
        transform.position += targetDirection * moveSpeed * Time.deltaTime;
    }
    
    private void Handle2DRotation()
    {
        // 이동 방향에 따른 각도 계산
        float targetAngle = Mathf.Atan2(targetDirection.y, Mathf.Abs(targetDirection.x)) * Mathf.Rad2Deg;
        
        // 각도 제한 (-maxRotationAngle ~ maxRotationAngle)
        targetAngle = Mathf.Clamp(targetAngle, -maxRotationAngle, maxRotationAngle);
        
        // 좌우 방향 처리
        if (targetDirection.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (targetDirection.x < 0 && facingRight)
        {
            Flip();
        }
        
        // 왼쪽을 보고 있을 때는 각도를 반전
        if (!facingRight)
        {
            targetAngle = -targetAngle;
        }
        
        // 부드러운 회전 적용
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f; // -180 ~ 180 범위로 정규화
        
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    private void HandleDirectionChange()
    {
        changeDirectionTimer -= Time.deltaTime;
        
        if (changeDirectionTimer <= 0f)
        {
            SetRandomDirection();
            changeDirectionTimer = Random.Range(changeDirectionInterval * 0.5f, changeDirectionInterval * 1.5f);
        }
    }
    
    private void CheckBoundaries()
    {
        Vector3 currentPos = transform.position;
        Vector3 boundsMin = initialPosition - new Vector3(movementBounds.x * 0.5f, movementBounds.y * 0.5f, 0);
        Vector3 boundsMax = initialPosition + new Vector3(movementBounds.x * 0.5f, movementBounds.y * 0.5f, 0);
        
        bool hitBoundary = false;
        Vector3 newDirection = targetDirection;
        
        // 경계 체크 및 방향 반전
        if (currentPos.x <= boundsMin.x && targetDirection.x < 0)
        {
            newDirection.x = Mathf.Abs(targetDirection.x);
            hitBoundary = true;
        }
        else if (currentPos.x >= boundsMax.x && targetDirection.x > 0)
        {
            newDirection.x = -Mathf.Abs(targetDirection.x);
            hitBoundary = true;
        }
        
        if (currentPos.y <= boundsMin.y && targetDirection.y < 0)
        {
            newDirection.y = Mathf.Abs(targetDirection.y);
            hitBoundary = true;
        }
        else if (currentPos.y >= boundsMax.y && targetDirection.y > 0)
        {
            newDirection.y = -Mathf.Abs(targetDirection.y);
            hitBoundary = true;
        }
        
        if (hitBoundary)
        {
            targetDirection = newDirection.normalized;
            changeDirectionTimer = changeDirectionInterval; // 타이머 리셋
        }
        
        // 영역 밖으로 나가지 않도록 위치 보정
        currentPos.x = Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x);
        currentPos.y = Mathf.Clamp(currentPos.y, boundsMin.y, boundsMax.y);
        transform.position = currentPos;
    }
    
    private void SetRandomDirection()
    {
        // 랜덤한 방향 설정 (정규화된 벡터)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        targetDirection = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0).normalized;
    }
    
    private void CatchTarget()
    {
        movementMode = FishMovementMode.Caught;
        // 물고기가 낚싯바늘을 물었을 때의 처리
        // 필요하다면 여기서 이벤트 호출 가능
    }
    
    // 공개 함수들
    public void SetTarget(Transform target)
    {
        currentTarget = target;
        if (target != null)
        {
            lastTargetPosition = target.position;
            SetMovementMode(FishMovementMode.FollowTarget);
        }
        else
        {
            SetMovementMode(FishMovementMode.Random);
        }
    }
    
    public void ClearTarget()
    {
        currentTarget = null;
        SetMovementMode(FishMovementMode.Random);
    }
    
    public void SetMovementMode(FishMovementMode mode)
    {
        movementMode = mode;
        
        if (mode == FishMovementMode.Random)
        {
            SetRandomDirection();
            changeDirectionTimer = changeDirectionInterval;
        }
    }
    
    public bool IsTargetInRange(Transform target)
    {
        if (target == null) return false;
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= targetDetectionRange;
    }
    
    public bool IsCaught()
    {
        return movementMode == FishMovementMode.Caught;
    }
    
    public void StartMovement()
    {
        isMoving = true;
    }
    
    public void StopMovement()
    {
        isMoving = false;
    }
    
    public void SetMovementBounds(Vector2 bounds)
    {
        movementBounds = bounds;
    }
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    // 디버그용 - Scene 뷰에서 움직임 영역 표시
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // 움직임 범위
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(initialPosition, new Vector3(movementBounds.x, movementBounds.y, 0));
            
            // 타겟 감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, targetDetectionRange);
            
            // 현재 타겟
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.position);
                Gizmos.DrawWireSphere(currentTarget.position, catchDistance);
            }
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(movementBounds.x, movementBounds.y, 0));
        }
    }
}
