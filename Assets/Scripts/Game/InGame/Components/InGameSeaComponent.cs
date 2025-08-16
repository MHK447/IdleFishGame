using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
public class InGameSeaComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject GradientObj;

    [SerializeField]
    private List<Transform> FishSpawnList = new List<Transform>();

    [SerializeField]
    private GameObject FishPrefab;



    private List<InGameFish> FishList = new List<InGameFish>();


    private int SeaIdx = 0;

    void Awake()
    {
        FishList.Clear();



    foreach (var fishspawn in FishSpawnList)
        {
            var fish = Instantiate(FishPrefab, transform).GetComponent<InGameFish>();

            fish.transform.SetParent(this.transform, false);
            fish.transform.localPosition = fishspawn.localPosition;

            FishList.Add(fish);

        }

    }

    public void Set(int seaidx)
    {
        var td = Tables.Instance.GetTable<SeaInfo>().GetData(seaidx);

        if (td != null)
        {
            SeaIdx = seaidx;

            ProjectUtility.SetActiveCheck(GradientObj, td.gradation_on == 1);

            for (int i = 0; i < FishSpawnList.Count; ++i)
            {
                var randfishidx = Random.Range(0, td.inhabit_fish.Count);

                FishList[i].Set(td.inhabit_fish[randfishidx]);
            }
        }
    }



    public InGameFish RandCatchFish()
    {
        // 활성화된 물고기들 중에서 아직 잡히지 않은 물고기들만 필터링
        var availableFishes = FishList.FindAll(x => x.gameObject.activeSelf && !x.IsCaught()).ToList();

        if (availableFishes.Count > 0)
        {
            int randomIndex = Random.Range(0, availableFishes.Count);
            return availableFishes[randomIndex];
        }

        return null;
    }

    public InGameFish GetClosestFish(Vector3 targetPosition)
    {
        // 활성화된 물고기들 중에서 아직 잡히지 않은 물고기들만 필터링
        var availableFishes = FishList.FindAll(x => x.gameObject.activeSelf && !x.IsCaught()).ToList();

        if (availableFishes.Count == 0)
            return null;

        InGameFish closestFish = null;
        float closestDistance = float.MaxValue;

        foreach (var fish in availableFishes)
        {
            float distance = Vector3.Distance(fish.transform.position, targetPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFish = fish;
            }
        }

        return closestFish;
    }

    public List<InGameFish> GetActiveFishes()
    {
        return FishList.FindAll(x => x.gameObject.activeSelf && !x.IsCaught()).ToList();
    }


    public float GetDistanceToPoint(Vector3 point)
    {
        return Vector3.Distance(transform.position, point);
    }
}
