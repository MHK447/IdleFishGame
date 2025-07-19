using System.Collections;
using System.Collections.Generic;
using BanpoFri;
using UnityEngine;

public class SelectItemContents : MonoBehaviour
{
    [SerializeField]
    private List<SelectItemComponent> ItemComponents = new List<SelectItemComponent>();

    public void Init()
    {
        
        foreach (var item in ItemComponents)
        {
            item.Set(1);
        }
    }
}
