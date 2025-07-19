using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
public class TutorialEntitySetText : TutorialEntity
{
    [SerializeField]
    private Text Text;
    [SerializeField]
    private string TextStr;

    public override void StartEntity()
    {
        base.StartEntity();

        Text.text = Tables.Instance.GetTable<Localize>().GetString(TextStr);
        Done();
    }
}
