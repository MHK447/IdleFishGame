using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFunction : MonoBehaviour
{
    public System.Action FuncAction;



    public void StartAnim()
    {
        FuncAction?.Invoke();
    }
}
