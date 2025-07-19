using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour
{
    [SerializeField] protected BoxCollider2D boxCollider;

    protected UnityAction clickCB;


    public void SetColliderSize()
    {

    }

    public void OnObjClicked()
    {
        clickCB?.Invoke();

    }

    public void SetClickAction(UnityAction clickAction)
    {
        this.clickCB = clickAction;
    }

    public void OnOffCollider(bool isOn)
    {
        boxCollider.enabled = isOn;
    }

}
