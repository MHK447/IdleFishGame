using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;
public class LocalizeString : MonoBehaviour
{
    public static List<LocalizeString> Localizelist { get; set; } = new List<LocalizeString>();
    [HideInInspector]
    [SerializeField]
    private string keyLocalize = "str_error";


    private void Start() {
        if(!Localizelist.Contains(this))
            Localizelist.Add(this);
        var tmppro = GetComponent<TextMeshProUGUI>();
        RefreshText();
    }
    public void RefreshText()
    {
            var tmppro = GetComponent<TextMeshProUGUI>();
            if (tmppro)
            {
                tmppro.text = Tables.Instance.GetTable<Localize>().GetString(keyLocalize);
            }
            else
            {
                var label = GetComponent<TextMeshProUGUI>();
                if (label)
                    label.text = Tables.Instance.GetTable<Localize>().GetString(keyLocalize);
            }
        
    }
  
    public void SetText(string txt)
    {
       
         
            var tmppro = GetComponent<TextMeshProUGUI>();
            if (tmppro)
            {
                tmppro.text = txt;
            }
        
    }
}
