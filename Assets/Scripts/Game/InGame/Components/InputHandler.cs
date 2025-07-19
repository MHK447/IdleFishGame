using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class InputHandler
{
    private int fixedUICount = 3; // Joystick_Head, Joystick, Joystick

    public void OnTouch(Vector2 mousePosition)
    {
        if (!IsUITouched())
        {
            //CheckRaycastTarget();
            CheckRaycastTarget2D(mousePosition);
        }
    }


    private bool IsUITouched()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Touch 제외한 UI 레이캐스트 없음
        return results.Count > fixedUICount;
    }


    private void CheckRaycastTarget()
    {
        RaycastHit hit;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            var clickableObj = hit.collider.GetComponentInParent<ClickableObject>();

            if (!ReferenceEquals(clickableObj, null))
            {
                clickableObj.OnObjClicked();
            }
        }

    }

    private void CheckRaycastTarget2D(Vector2 mousePosition)
    {
        Vector2 touchPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Ray2D ray = new Ray2D(touchPosition, Vector2.zero);

        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);

        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit2D hit = hits[i];

            if (hit.collider == null)
            {
                continue;
            }

            var clickableObj = hit.collider.GetComponentInParent<ClickableObject>();

            if (!ReferenceEquals(clickableObj, null))
            {
                clickableObj.OnObjClicked();

                break;
            }
        }
    }
}
