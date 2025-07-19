using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltContents : MonoBehaviour
{
    [SerializeField]
    private List<VoltComponent> VoltComponents = new List<VoltComponent>();



    public void Set(int order)
    {
        foreach(var volt in VoltComponents)
        {
            volt.Init();
        }
        
        

    }
}
