using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TutorialEntityHoldToMove : TutorialEntity
{
    [SerializeField]
    private Slider Slider;

    PanAndZoom Cam;

    private float touchtime = 0f;

    private float speed = 0.5f;
    public override void StartEntity()
    {
        base.StartEntity();

        Slider.value = 0f;

        Cam = GameRoot.Instance.InGameSystem.CurInGame.IngameCamera;

    }


    private void Update()
    {
        if (Cam == null) return;


        if(Cam.isTouching)
        {
            touchtime += Time.deltaTime * speed;

            Slider.value = touchtime;


            if (touchtime >= 1f)
            {
                Done();
            }
        }
        else
        {
            Slider.value = 0f;
            touchtime = 0f;
        }
    }

}
