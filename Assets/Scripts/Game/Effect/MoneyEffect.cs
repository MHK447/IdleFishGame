using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using DG.Tweening;

[EffectPath("Effect/MoneyEffect", false, false)]
public class MoneyEffect : Effect
{   
    public void Init(Transform target, System.Action endaction)
    {
        this.transform.DOMove(target.transform.position, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
            endaction?.Invoke();
        });
    }   
}
