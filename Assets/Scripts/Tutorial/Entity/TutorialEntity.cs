using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;

public class TutorialEntity : MonoBehaviour
{
    public bool Complete { get; set; } = false;
    [SerializeField]
    protected List<GameObject> activeList = new List<GameObject>();
    [SerializeField]
    protected List<GameObject> IgnoreDeactiveList = new List<GameObject>();

    public virtual void StartEntity()
    {
        foreach (var active in activeList)
        {
            ProjectUtility.SetActiveCheck(active, true);
        }

        TpLog.Log(this.name);
    }

    protected virtual void Done()
    {
        foreach (var active in activeList)
        {
            if(!IgnoreDeactiveList.Contains(active))
                ProjectUtility.SetActiveCheck(active, false);
        }
        Complete = true;
    }
}
