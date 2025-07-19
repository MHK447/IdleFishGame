using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BanpoFri;
using System.Linq;


public class LobbyFoodComponentGroup : MonoBehaviour
{
    [SerializeField]
    private List<LobbyStageFoodGroupComponent> LobbyStageFoodGroupComponentList = new List<LobbyStageFoodGroupComponent>();

    [SerializeField]
    private TextMeshProUGUI StageNameText;

    private int StageIdx = 0;

    public void Set(int stageidx)
    {
        StageIdx = stageidx;

        var tdlist = Tables.Instance.GetTable<FoodMergeGroupInfo>().DataList.FindAll(x => x.stageidx == stageidx).ToList();

        for (int i = 0; i < tdlist.Count; i++)
        {
            ProjectUtility.SetActiveCheck(LobbyStageFoodGroupComponentList[i].gameObject, true);
            LobbyStageFoodGroupComponentList[i].Set(tdlist[i].mergeidx);

        


            StageNameText.text = $"{Tables.Instance.GetTable<Localize>().GetString($"str_stagemap_{StageIdx}")}";
        }
    }
}
