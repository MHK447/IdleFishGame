using UnityEngine;
using UnityEngine.UI;
using BanpoFri;

public class AquariumFishComponent : MonoBehaviour
{
    [SerializeField]
    private Image fishImage;

    private int FishIdx = 0;



    public void Set(int idx)
    {
        FishIdx = idx;

        var td = Tables.Instance.GetTable<FishInfo>().GetData(FishIdx);

        if (td != null)
        {
            fishImage.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_InGameFish, $"Ingame_Fish_{FishIdx}");
        }
    }

}
