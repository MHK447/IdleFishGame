using UnityEngine;
using BanpoFri;


public class InGameFish : MonoBehaviour
{
    private int FishIdx = 0;


    public int GetFishIdx { get { return FishIdx; } }


    public void Set(int fishidx)
    {
        FishIdx = fishidx;
    }


    public void ReturnSpawner()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, false);
    }
}
