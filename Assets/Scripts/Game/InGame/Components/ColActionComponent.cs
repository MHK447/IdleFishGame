using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColActionComponent : MonoBehaviour
{
    public System.Action<Collider2D> TriggerEnterEvent;
    public System.Action<Collider2D> TriggerExitEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerEnterEvent?.Invoke(collision);
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        TriggerExitEvent?.Invoke(collision);
    }
}
