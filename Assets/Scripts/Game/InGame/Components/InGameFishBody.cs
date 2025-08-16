using BanpoFri;
using UnityEngine;

public enum FishMovementMode
{
    Random,      // 랜덤 움직임
    FollowTarget, // 타겟 추적
    Caught       // 낚싯바늘을 물어서 정지
}

// 물고기 움직임 타입 열거형 추가
public enum FishMovePattern
{
    NormalFish = 1,  // 일반 물고기 (빠르고 활발한 움직임)
    WhaleType = 2    // 고래 타입 (느리고 우아한 움직임)
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

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 1f; // 잡힌 후 다시 활동하기까지의 시간
    [SerializeField] private float respawnFadeTime = 1f; // 페이드 인 시간

    [Header("Whale Type Settings")]
    [SerializeField] private float whaleSpeedMultiplier = 0.6f; // 고래 타입 속도 배율
    [SerializeField] private float whaleDirectionChangeMultiplier = 2.0f; // 고래 타입 방향 전환 간격 배율
    [SerializeField] private float whaleRotationSpeedMultiplier = 0.5f; // 고래 타입 회전 속도 배율
    [SerializeField] private float whaleSmoothMovement = 0.1f; // 고래 타입 부드러운 움직임

    private Vector3 targetDirection;
    private Vector3 initialPosition;
    private float changeDirectionTimer;
    private bool isMoving = false;
    private bool facingRight = true; // 물고기가 오른쪽을 보고 있는지

    // 타겟 추적 관련
    private FishMovementMode movementMode = FishMovementMode.Random;
    private Transform currentTarget;
    private Vector3 lastTargetPosition;
    private System.Action<int> onCaughtCallback; // 잡혔을 때 호출할 콜백

    // 바다 이동 추적용
    private Vector3 lastParentPosition;
    private bool hasParent = false;

    // 리스폰 관련
    private float respawnTimer = 0f;
    private bool isRespawning = false;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private int FishIdx = 0;
    private int FishMoveType = 0;

    // 고래 타입 전용 변수들
    private Vector3 whaleCurrentVelocity;
    private Vector3 whaleTargetDirection;

    // 현재 물고기 타입별 설정값들
    private float currentMoveSpeed;
    private float currentChangeDirectionInterval;
    private float currentRotationSpeed;

    void Start()
    {
        InitializeFish();
    }

    void Update()
    {
        if (isRespawning)
        {
            HandleRespawn();
            return;
        }

        // 바다(부모) 이동 감지 및 처리
        HandleParentMovement();

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

    public void Init(int fishidx)
    {
        FishIdx = fishidx;

        spriteRenderer.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_InGameFish, $"Ingame_Fish_{fishidx}");

        var td = Tables.Instance.GetTable<FishInfo>().GetData(fishidx);

        if (td != null)
        {
            FishMoveType = td.move_type;
            ApplyMoveTypeSettings();
        }
    }

    private void ApplyMoveTypeSettings()
    {
        switch (FishMoveType)
        {
            case (int)FishMovePattern.NormalFish:
                currentMoveSpeed = moveSpeed;
                currentChangeDirectionInterval = changeDirectionInterval;
                currentRotationSpeed = rotationSpeed;
                break;

            case (int)FishMovePattern.WhaleType:
                currentMoveSpeed = moveSpeed * whaleSpeedMultiplier;
                currentChangeDirectionInterval = changeDirectionInterval * whaleDirectionChangeMultiplier;
                currentRotationSpeed = rotationSpeed * whaleRotationSpeedMultiplier;
                whaleCurrentVelocity = Vector3.zero;
                whaleTargetDirection = Vector3.right;
                break;

            default:
                // 기본값 사용
                currentMoveSpeed = moveSpeed;
                currentChangeDirectionInterval = changeDirectionInterval;
                currentRotationSpeed = rotationSpeed;
                break;
        }
    }

    private void InitializeFish()
    {
        initialPosition = transform.position;
        
        // 물고기 타입에 따른 초기 방향 설정
        FishMovePattern movePattern = (FishMovePattern)FishMoveType;
        if (movePattern == FishMovePattern.WhaleType)
        {
            SetWhaleRandomDirection();
            targetDirection = whaleTargetDirection;
        }
        else
        {
            SetRandomDirection();
        }
        
        changeDirectionTimer = currentChangeDirectionInterval;
        isMoving = true;
        movementMode = FishMovementMode.Random;

        // 부모(바다) 위치 초기화
        if (transform.parent != null)
        {
            lastParentPosition = transform.parent.position;
            hasParent = true;
        }

        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }

    private void HandleRandomMovement()
    {
        FishMovePattern movePattern = (FishMovePattern)FishMoveType;

        switch (movePattern)
        {
            case FishMovePattern.NormalFish:
                HandleNormalFishMovement();
                break;

            // case FishMovePattern.WhaleType:
            //     HandleWhaleMovement();
            //     break;

            default:
                HandleNormalFishMovement();
                break;
        }

        CheckBoundaries();
    }

    private void HandleNormalFishMovement()
    {
        // 기존 일반 물고기 움직임
        HandleMovement();
        HandleDirectionChange();
    }

    private void HandleWhaleMovement()
    {
        // 고래 타입의 부드럽고 우아한 움직임
        HandleWhaleDirectionChange();
        HandleWhaleSmoothMovement();
        
        // 고래 타입 전용 경계 체크
        Vector3 currentPos = transform.position;
        Vector3 boundsMin = initialPosition - new Vector3(movementBounds.x * 0.5f, movementBounds.y * 0.5f, 0);
        Vector3 boundsMax = initialPosition + new Vector3(movementBounds.x * 0.5f, movementBounds.y * 0.5f, 0);
        CheckWhaleBoundaries(currentPos, boundsMin, boundsMax);
    }

    private void HandleWhaleDirectionChange()
    {
        changeDirectionTimer -= Time.deltaTime;

        if (changeDirectionTimer <= 0f)
        {
            SetWhaleRandomDirection();
            changeDirectionTimer = Random.Range(currentChangeDirectionInterval * 0.8f, currentChangeDirectionInterval * 1.2f);
        }
    }

    private void HandleWhaleSmoothMovement()
    {
        // 부드러운 가속/감속 움직임
        whaleCurrentVelocity = Vector3.Lerp(whaleCurrentVelocity, whaleTargetDirection * currentMoveSpeed, whaleSmoothMovement);
        transform.position += whaleCurrentVelocity * Time.deltaTime;
        
        // 회전용으로 실제 움직이는 방향 업데이트 (움직임이 있을 때만)
        if (whaleCurrentVelocity.magnitude > 0.1f)
        {
            targetDirection = whaleCurrentVelocity.normalized;
        }
        else
        {
            // 움직임이 거의 없으면 목표 방향 사용
            targetDirection = whaleTargetDirection;
        }
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

        // 타겟 추적 속도로 이동 (타입별 속도 적용)
        float followSpeed = targetFollowSpeed;
        if (FishMoveType == (int)FishMovePattern.WhaleType)
        {
            followSpeed *= whaleSpeedMultiplier;
        }
        transform.position += targetDirection * followSpeed * Time.deltaTime;

        lastTargetPosition = targetPos;
    }

    private void HandleCaughtState()
    {
        // 낚싯바늘을 물었을 때는 타겟을 따라다님
        if (currentTarget != null)
        {
            Vector3 targetPos = currentTarget.position;
            transform.position = targetPos;

            // 타겟 방향 설정 (회전용)
            Vector3 directionToTarget = (targetPos - lastTargetPosition).normalized;
            if (directionToTarget.magnitude > 0.1f)
            {
                targetDirection = directionToTarget;
            }

            lastTargetPosition = targetPos;
        }
    }

    private void HandleRespawn()
    {
        // 페이드 아웃 후 리스폰
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * 3f);
            spriteRenderer.color = color;

            // 완전히 투명해지면 리스폰 위치로 이동
            if (color.a < 0.1f)
            {
                RespawnAtRightSide();
            }
        }
        else
        {
            // SpriteRenderer가 없으면 바로 리스폰
            RespawnAtRightSide();
        }
    }

    private void StartRespawn()
    {
        isRespawning = true;
        respawnTimer = 0f;
    }

    private void RespawnAtRightSide()
    {
        // 오른쪽 경계에서 리스폰
        Vector3 respawnPos = initialPosition;
        respawnPos.x = initialPosition.x + movementBounds.x * 0.5f; // 오른쪽 경계
        respawnPos.y = initialPosition.y + Random.Range(-movementBounds.y * 0.3f, movementBounds.y * 0.3f); // 약간의 Y 변화

        transform.position = respawnPos;

        // 왼쪽을 보도록 설정
        if (facingRight)
        {
            Flip();
        }

        // 왼쪽 방향으로 초기 이동 설정
        targetDirection = Vector3.left;

        // 상태 리셋
        currentTarget = null;
        movementMode = FishMovementMode.Random;
        isRespawning = false;

        // 페이드 인 시작
        StartCoroutine(FadeIn());

        // 움직임 재시작
        changeDirectionTimer = currentChangeDirectionInterval;
    }

    private System.Collections.IEnumerator FadeIn()
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            float elapsed = 0f;
            while (elapsed < respawnFadeTime)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsed / respawnFadeTime);
                spriteRenderer.color = color;
                yield return null;
            }

            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    private void HandleMovement()
    {
        // 현재 방향으로 이동
        transform.position += targetDirection * currentMoveSpeed * Time.deltaTime;
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

        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, currentRotationSpeed * Time.deltaTime);
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
            changeDirectionTimer = Random.Range(currentChangeDirectionInterval * 0.5f, currentChangeDirectionInterval * 1.5f);
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
        respawnTimer = 0f; // 타이머 초기화

        // 물고기가 낚싯바늘을 물었을 때 콜백 호출
        onCaughtCallback?.Invoke(FishIdx);
        onCaughtCallback = null; // 한 번만 호출하도록
    }

    // 공개 함수들
    public void SetTarget(Transform target)
    {
        SetTarget(target, null);
    }

    public void SetTarget(Transform target, System.Action<int> onCaught)
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
        onCaughtCallback = onCaught;
    }

    public void ClearTarget()
    {
        currentTarget = null;
        SetMovementMode(FishMovementMode.Random);
        onCaughtCallback = null;
    }

    public void SetMovementMode(FishMovementMode mode)
    {
        movementMode = mode;

        if (mode == FishMovementMode.Random)
        {
            // 물고기 타입에 따른 방향 설정
            FishMovePattern movePattern = (FishMovePattern)FishMoveType;
            if (movePattern == FishMovePattern.WhaleType)
            {
                SetWhaleRandomDirection();
                targetDirection = whaleTargetDirection;
            }
            else
            {
                SetRandomDirection();
            }
            changeDirectionTimer = currentChangeDirectionInterval;
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
        ApplyMoveTypeSettings(); // 타입별 설정 다시 적용
    }

    // 즉시 리스폰 (외부에서 호출 가능)
    public void ForceRespawn()
    {
        StartRespawn();
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

            // 리스폰 위치
            if (isRespawning)
            {
                Vector3 respawnPos = initialPosition;
                respawnPos.x = initialPosition.x + movementBounds.x * 0.5f;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(respawnPos, 0.5f);
            }
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(movementBounds.x, movementBounds.y, 0));
        }
    }

    private void CheckWhaleBoundaries(Vector3 currentPos, Vector3 boundsMin, Vector3 boundsMax)
    {
        // 고래 타입의 부드러운 경계 처리 (양옆으로만 움직임)
        float boundaryBuffer = 1.0f; // 경계에서 부드럽게 회전하기 시작하는 거리
        
        bool needRedirection = false;
        
        // X축 경계 체크만 (양옆 움직임)
        if (currentPos.x <= boundsMin.x + boundaryBuffer && whaleTargetDirection.x < 0)
        {
            whaleTargetDirection.x = Mathf.Lerp(whaleTargetDirection.x, Mathf.Abs(whaleTargetDirection.x), Time.deltaTime * 2f);
            needRedirection = true;
        }
        else if (currentPos.x >= boundsMax.x - boundaryBuffer && whaleTargetDirection.x > 0)
        {
            whaleTargetDirection.x = Mathf.Lerp(whaleTargetDirection.x, -Mathf.Abs(whaleTargetDirection.x), Time.deltaTime * 2f);
            needRedirection = true;
        }
        
        // Y축은 중앙 근처에서 유지 (약간의 상하 움직임만 허용)
        float centerY = (boundsMin.y + boundsMax.y) * 0.5f;
        float yOffset = currentPos.y - centerY;
        if (Mathf.Abs(yOffset) > movementBounds.y * 0.1f) // 전체 높이의 10% 범위 내에서만 유지
        {
            whaleTargetDirection.y = -yOffset * 0.1f; // 중앙으로 천천히 복귀
            needRedirection = true;
        }
        
        if (needRedirection)
        {
            whaleTargetDirection = whaleTargetDirection.normalized;
            targetDirection = whaleTargetDirection; // 방향 동기화
            changeDirectionTimer = currentChangeDirectionInterval * 0.5f; // 빠른 방향 전환
        }
    }

    private void SetWhaleRandomDirection()
    {
        // 고래는 양옆으로만 느릿느릿 움직임
        if (whaleTargetDirection == Vector3.zero)
        {
            // 초기화 시에는 왼쪽 또는 오른쪽 방향
            float randomDirection = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
            whaleTargetDirection = new Vector3(randomDirection, 0f, 0f).normalized;
        }
        else
        {
            // 현재 X 방향을 기준으로 좌우 방향만 변경
            // 가끔씩 방향을 바꾸거나 유지
            if (Random.Range(0f, 1f) < 0.3f) // 30% 확률로 방향 변경
            {
                whaleTargetDirection.x = -whaleTargetDirection.x; // 반대 방향으로
            }
            // Y축은 거의 0에 가깝게 유지 (약간의 상하 움직임은 허용)
            whaleTargetDirection.y = Random.Range(-0.1f, 0.1f);
            whaleTargetDirection = whaleTargetDirection.normalized;
        }
    }

    private void HandleParentMovement()
    {
        if (!hasParent || transform.parent == null)
            return;

        Vector3 currentParentPosition = transform.parent.position;

        // 부모가 이동했는지 확인
        if (Vector3.Distance(currentParentPosition, lastParentPosition) > 0.01f)
        {
            // 부모가 이동한 만큼 물고기의 기준점들도 이동
            Vector3 parentMoveDelta = currentParentPosition - lastParentPosition;

            // 물고기의 initialPosition 기준 상대적 위치 계산
            Vector3 relativePosition = transform.position - initialPosition;

            // initialPosition 업데이트
            initialPosition += parentMoveDelta;

            // 새로운 initialPosition 기준으로 상대적 위치 유지
            transform.position = initialPosition + relativePosition;

            // 부모 위치 업데이트
            lastParentPosition = currentParentPosition;
        }
    }
}
