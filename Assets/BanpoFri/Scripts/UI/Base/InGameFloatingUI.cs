using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;


public class InGameFloatingUI : MonoBehaviour, IFloatingUI
{   
    [SerializeField]
    private bool TrackingScale = false;
    [SerializeField]
    private bool TrackingPos = false;
    protected Transform FollowTrans = null;
    [SerializeField]
    private Vector3 OffsetVec;

    public virtual void Init(Transform parent)
    {
        FollowTrans = parent;   
        this.transform.position = FollowTrans.position + OffsetVec;
    }

    public void UpdatePos()
    {
        if (FollowTrans != null)
            this.transform.position = FollowTrans.position + OffsetVec;
    }

    public void SetUpdatePos(Vector3 position)
    {
        this.transform.position = position + OffsetVec;
    }

    public void SetOffset(Vector3 pos)
    {
        OffsetVec = pos;
    }


    public void SetLocalUpdatePos(Vector3 position)
    {
        this.transform.localPosition = position + OffsetVec;
    }

    protected virtual void Update()
    {
        if (TrackingPos)
        {
            if (FollowTrans != null)
                this.transform.position = FollowTrans.position + OffsetVec;
        }

        if (TrackingScale)
        {
            if (FollowTrans != null)
                this.transform.localScale = FollowTrans.localScale;
        }


        if(FollowTrans == null)
        {
            this.gameObject.SetActive(false);
        }
    }
}
