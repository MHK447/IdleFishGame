using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using BanpoFri;
using UniRx;
using TMPro;


public class SelectItemComponent : MonoBehaviour
{
    [SerializeField]
    private Image ItemImage;

    [SerializeField]
    private TextMeshProUGUI PriceText;
    
    private int SelectItemIdx = 0;

    [SerializeField]
    private StoreBuyProductComponent StoreBuyProductComponent;

    public void Set(int selectitemidx)
    {
        SelectItemIdx = selectitemidx;

        StoreBuyProductComponent.Init();
    }



}
