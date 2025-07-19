using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;

public class VoltComponent : MonoBehaviour
{
    [HideInInspector]
    private StoreBuyProductComponent ProductComponent;


    [SerializeField]
    private int Order = 0;





    public void Init()
    {
        
    }





    public void OnProduct(StoreBuyProductComponent productComponent)
    {
        ProductComponent = productComponent;
        ProductComponent.transform.SetParent(transform);
        ProductComponent.transform.position = Vector3.zero;
        ProductComponent.Init();
    }

    public void OffProduct()
    {
        ProductComponent = null;

    }
}
