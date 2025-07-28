using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    private List<InGameFish> FishList = new List<InGameFish>();


    public InGameFish SpawnFish(int fishidx)
    {
        var findfish = FishList.Find(x=> x.gameObject.activeSelf == false && x.GetFishIdx == fishidx);

        if(findfish == null)
        {
            findfish = Instantiate(FishList[fishidx], transform);
            FishList.Add(findfish);
        }
        
        return findfish;
    }

}
