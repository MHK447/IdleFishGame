using UnityEngine;

public class FishingHookComponent : MonoBehaviour
{
    [SerializeField]
    private Transform FisshingHookObj;

    public Transform FisshingHookTr { get { return FisshingHookObj; } }

    [SerializeField]
    private LineRenderer LineRenderer;

    [SerializeField]
    private Transform LinrStartTr;

    [SerializeField]
    private Transform LineEndTr;

    private Vector3 StartPos;

    private float SeaDepth = -10f;

    private float MinHookY = -10f;

    public float metersPerUnit = 0.1f;  // 10 유닛 = 1m → 1 유닛 = 0.1m

    void Awake()
    {
        StartPos = transform.position;

        LineRenderer.positionCount = 2;
        LineRenderer.startWidth = 0.15f;
        LineRenderer.endWidth = 0.15f;
    }


    public void Init()
    {
        this.transform.position = StartPos;
    }


    void Update()
    {
        float currentY = transform.position.y < MinHookY ? MinHookY : transform.position.y;

        GameRoot.Instance.PlayerSystem.SeaDepthProperty.Value = (StartPos.y - currentY) * (1f / metersPerUnit);

        // 낚시줄 길이 조정
        UpdateLineLength();
    }

    private void UpdateLineLength()
    {
        LineRenderer.SetPosition(0, LinrStartTr.position);
        LineRenderer.SetPosition(1, LineEndTr.position);
    }
}
