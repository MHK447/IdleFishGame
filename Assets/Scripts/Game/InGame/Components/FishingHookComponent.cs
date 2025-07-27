using UnityEngine;

public class FishingHookComponent : MonoBehaviour
{
    private Vector3 StartPos;

    private float SeaDepth = -10f;


    public float metersPerUnit = 0.1f;  // 10 유닛 = 1m → 1 유닛 = 0.1m

    void Awake()
    {
        StartPos = transform.position;
    }


    public void Init()
    {
        this.transform.position = StartPos;

    }


    void Update()
    {
        float currentY = transform.position.y;

        GameRoot.Instance.PlayerSystem.SeaDepthProperty.Value = (StartPos.y - currentY) * (1f / metersPerUnit);

    }
}
