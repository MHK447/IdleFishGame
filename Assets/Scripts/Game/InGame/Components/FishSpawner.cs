using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FishSpawner : MonoBehaviour
{
    private List<InGameFish> FishList = new List<InGameFish>();

    [SerializeField]
    private GameObject FishPrefab;

    private int MaxCleanCount = 100;

    public InGameFish SpawnFish(int fishidx)
    {
        FishSpawnerClean();

        var findfish = FishList.Find(x=> x.gameObject.activeSelf == false && x.GetFishIdx == fishidx);

        if(findfish == null)
        {
            findfish = Instantiate(FishPrefab, transform).GetComponent<InGameFish>();

            FishList.Add(findfish);
        }

        return findfish;
    }


    public void FishSpawnerClean()
    {
        if(FishList.Count > MaxCleanCount)
        {
            var fishlist = FishList.FindAll(x=> x.gameObject.activeSelf == false).ToList();

            foreach(var fish in fishlist)
            {
                FishList.Remove(fish);
                Destroy(fish.gameObject);
            }
        }
    }

}
