using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;


[UIPath("UI/Page/PageShop")]
public class PageShop : UIBase
{

    [SerializeField]
    private List<ShopProductComponent> ProductComponents = new List<ShopProductComponent>();


    public void Init()
    { 
        foreach(var product in ProductComponents)
        {
            product.Init();
        }
    }
}
