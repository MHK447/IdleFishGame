using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using BanpoFri;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InGameSea : MonoBehaviour
{
    [SerializeField]
    private List<Transform> FishSpawnList = new List<Transform>();


    private List<InGameFish> FishList = new List<InGameFish>();

    private int Depth = 0;

    private InGameScrollSea ScrollSea;

    public void Set(int seaidx)
    {
        ScrollSea = GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().GetScrollSea;

        foreach (var fish in FishList)
        {
            fish.ReturnSpawner();
        }

        FishList.Clear();

        var td = Tables.Instance.GetTable<SeaInfo>().GetData(seaidx);

        if (td != null)
        {

            Depth = td.depth;

            var tdfishlist = Tables.Instance.GetTable<FishInfo>().DataList.FindAll(x => x.max_depth > Depth && x.min_depth <= Depth).ToList();

            foreach (var fishspawn in FishSpawnList)
            {
                var randvalue = Random.Range(0, tdfishlist.Count);

                var fish = ScrollSea.FishSpawner.SpawnFish(tdfishlist[randvalue].idx);

                fish.transform.SetParent(this.transform, false);
                fish.transform.localPosition = fishspawn.localPosition;

                FishList.Add(fish);

                fish.Set(tdfishlist[randvalue].idx);

                ProjectUtility.SetActiveCheck(fish.gameObject, true);


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
