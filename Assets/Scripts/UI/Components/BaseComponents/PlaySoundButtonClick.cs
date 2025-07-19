using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlaySoundButtonClick : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    private string keySound = string.Empty;

    private void Awake() {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(() => {
            SoundPlayer.Instance.PlaySound(keySound);
        });
    }
}
