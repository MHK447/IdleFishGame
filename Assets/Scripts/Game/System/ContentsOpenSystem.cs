using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;

public class ContentsOpenSystem : MonoBehaviour
{
    public enum ContentsOpenType
    {
        Interstitial = 1,
    }


    public bool ContentsOpenCheck(ContentsOpenType opentype)
    {
        bool isopencheck = false;

        var td = Tables.Instance.GetTable<ContentsOpenCheck>().GetData((int)opentype);

        //var stageidx = GameRoot.Instance.UserData.Stagedata.Stageidx.Value;

        if(td != null)
        {
           

            // if(stageidx > td.stage_idx)
            // {
            //     isopencheck = true;
            // }
            // // else if(stageidx == td.stage_idx && findfacility.IsOpen)
            // {
            //     isopencheck = true;
            // }
        }

        return isopencheck;

    }


}
