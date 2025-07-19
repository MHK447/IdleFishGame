using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using DG.Tweening;
using UnityEngine.UI;

[UIPath("UI/Page/PageFade")]
public class PageFade : UIBase
{

    private Sequence sequenceFadeInOut;


    [SerializeField]
    private CanvasGroup FadeImg;

    private int Time = 1;

    private System.Action FadeAction = null;

    public override void CustomSortingOrder()
    {
        base.CustomSortingOrder();

        transform.GetComponent<Canvas>().sortingOrder = 32767;
    }

    public void Set(System.Action fadeon)
    {
        FadeAction = fadeon;

        FadeImg.alpha = 0f;
        if (sequenceFadeInOut != null)
        {
            sequenceFadeInOut.Restart();
        }
        else
        {
            sequenceFadeInOut = DOTween.Sequence()
        .SetAutoKill(false)
        .Append(FadeImg.DOFade(1.0f, 1f))
        .AppendCallback(() => {
            FadeAction?.Invoke();
            FadeAction = null;
        })// 어두워짐. 알파 값 조정.
        .Append(FadeImg.DOFade(0.0f, 1f)) // 밝아짐. 알파 값 조정.
        .OnComplete(() => // 실행 후.
        {
            FadeImg.alpha = 0f;
            Hide();
        });
        }
    }

}
