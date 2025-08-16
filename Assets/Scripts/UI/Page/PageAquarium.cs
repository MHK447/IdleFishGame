using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using System.Collections.Generic;

[UIPath("UI/Page/PageAquarium", true)]
public class PageAquarium : UIBase
{
    private List<GameObject> FishComponents = new List<GameObject>();

    [SerializeField]
    private GameObject FishPrefab;

    [SerializeField]
    private Transform FishRoot;



    public void Init()
    {
        foreach(var fishcomp in FishComponents)
        {
            ProjectUtility.SetActiveCheck(fishcomp, false);
        }

        foreach (var fishidx in GameRoot.Instance.UserData.Aquariumdata.Fishidxs)
        {
            var td = Tables.Instance.GetTable<FishInfo>().GetData(fishidx);

            if (td != null)
            {
                var inst = GetCachedObject().GetComponent<AquariumFishComponent>();

                if(inst != null)
                {
                    inst.Set(fishidx);

                    ProjectUtility.SetActiveCheck(inst.gameObject, true);
                }
            }
        }
    }



    private GameObject GetCachedObject()
    {
        var inst = FishComponents.Find(x => !x.gameObject.activeSelf);
        if (inst == null)
        {
            inst = GameObject.Instantiate(FishPrefab);
            inst.transform.SetParent(FishRoot);
            inst.transform.localScale = UnityEngine.Vector3.one;
            FishComponents.Add(inst);
        }

        return inst;
    }

}
