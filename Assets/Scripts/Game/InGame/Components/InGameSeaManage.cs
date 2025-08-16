using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using BanpoFri;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InGameSeaManage : MonoBehaviour
{

    [SerializeField]
    private int SeaIdx = 0;

    [SerializeField]
    private List<InGameSeaComponent> SeaListPrefab = new List<InGameSeaComponent>();

    private InGameSeaComponent CurrentSeaObj;

    public InGameSeaComponent GetCurrentSea { get { return CurrentSeaObj; } }


    private List<InGameSeaComponent> CurrentSeaList = new List<InGameSeaComponent>();

    public int GetSeaidx { get { return SeaIdx; } }


    private int Depth = 0;

    private InGameScrollSea ScrollSea;

    public void Init()
    {
        CurrentSeaList.Clear();
    }

    public void Set(int seaidx)
    {
        SeaIdx = seaidx;

        ScrollSea = GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().GetScrollSea;

        var td = Tables.Instance.GetTable<SeaInfo>().GetData(seaidx);

        if (td != null)
        {
            SeaCreate(seaidx);
        }
    }


    public void SeaCreate(int seaidx)
    {
        var td = Tables.Instance.GetTable<SeaInfo>().GetData(seaidx);

        if (td != null)
        {
            if(CurrentSeaObj != null)
            {
                ProjectUtility.SetActiveCheck(CurrentSeaObj.gameObject , false);
            }


            var findsea = CurrentSeaList.Find(x => !x.gameObject.activeSelf);

            if (findsea != null)
            {
                ProjectUtility.SetActiveCheck(findsea.gameObject, true);
                findsea.Set(seaidx);

                CurrentSeaObj = findsea;
            }
            else
            {
                var sea = Instantiate(SeaListPrefab[seaidx - 1], transform);

                CurrentSeaObj = sea;
                sea.Set(seaidx);
                CurrentSeaList.Add(sea);
            }
        }
    }


}
