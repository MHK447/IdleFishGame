using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using BanpoFri;
using UniRx;
using TMPro;
using UnityEngine.UI;

public class StoreBuyProductComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Image ItemImage;

    [SerializeField]
    private Vector2 Offset;

    [SerializeField]
    private Vector2 WeaponOffset;

    private RectTransform RecT;

    [HideInInspector]
    public bool IsDraggingStart = false;


    public void Init()
    {
        RecT = ItemImage.transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
        IsDraggingStart = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");

        if (IsDraggingStart)
        {
            MoveToMousePosition(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");


        IsDraggingStart = false;
    }




    private void MoveToMousePosition(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            RecT.anchoredPosition = localPoint - Offset - WeaponOffset;
        }
    }

}
