using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[EffectPath("Effect/TextEffectMax", false, true)]
public class TextEffectMax : Effect
{

    [SerializeField]
    private Text MaxText;

    private Transform TargetTr;

    public void Init(Transform Tr)
    {
        TargetTr = Tr;

        MaxText.text = "MAX";
    }



    private void Update()
    {
        if (TargetTr == null) return;

        this.transform.position = TargetTr.position;
    }
}
