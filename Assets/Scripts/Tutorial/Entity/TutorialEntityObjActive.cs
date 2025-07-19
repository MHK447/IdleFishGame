using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BanpoFri;
public class TutorialEntityObjActive : TutorialEntity
{
    [SerializeField]
    TutorialIdent id;
    [SerializeField]
    private int targetIdx = -1;
    [SerializeField]
    private int targetSubIdx = -1;
    [SerializeField]
    bool active;

    GameObject target;

    public override void StartEntity()
    {
        base.StartEntity();

        if (id != TutorialIdent.None)
        {
            if (GameRoot.Instance.TutorialSystem.IsDynamaicTarget(id))
            {
                target = GameRoot.Instance.TutorialSystem.GetTarget(id, targetIdx, targetSubIdx);
            }
            else
            {
                var register = GameRoot.Instance.TutorialSystem.GetRegister(id);
                if (register == null)
                    return;

                target = register.Target;
            }
        }

        ProjectUtility.SetActiveCheck(target, active);

        switch (id)
        {
            case TutorialIdent.NextStageBtn:
                {
                    break;
                }
        }
        Done();
    }

}
