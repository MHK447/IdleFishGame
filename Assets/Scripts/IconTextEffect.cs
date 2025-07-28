using System.Collections;
using System.Collections.Generic;
using BanpoFri;
using UnityEngine;
using UnityEngine.UI;

[EffectPath("Effect/IconText", false, true)]
public class IconTextEffect : Effect
{
    [SerializeField]
    private Animator ani;
    [SerializeField]
    private Text text;

    public void SetText(string _text, bool playAni = true)
    {
        text.text = $"+{_text}";
        if (playAni)
            ani.Play("Show", 0, 0f);

        SetAutoRemove(true, 1f);
    }

    void HideActive(){
        ProjectUtility.SetActiveCheck(gameObject , false);
    }

    public void SetText_NotRemove(string _text){
        text.text = $"+{_text}";
        ani.Play("Show", 0, 0f);
    }

    public void Reset()
    {
        ani?.Rebind();
    }
}
