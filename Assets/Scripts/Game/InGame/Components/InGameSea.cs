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
}
