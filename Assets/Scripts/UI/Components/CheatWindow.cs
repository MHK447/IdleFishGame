using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BanpoFri;
using System;

public class CheatWindow : MonoBehaviour
{
    [SerializeField]
    private InputField inputField;

    public void SetMoney()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            TpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            TpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.CurMode.Money.Value += convert;
        GameRoot.Instance.UserData.HUDMoney.Value += convert;
    }


    public void SetEnergeyMoney()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            TpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            TpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.CurMode.EnergyMoney.Value += convert;
    }

    public void SetCash()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            TpLog.LogError("input field empty!");
            return;
        }

        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            TpLog.LogError("input field string don't convert number!");
            return;
        }

        inputField.text = "";

        if (convert > int.MaxValue || (convert + GameRoot.Instance.UserData.Cash.Value) > int.MaxValue)
        {
            GameRoot.Instance.UserData.Cash.Value = int.MaxValue;
            GameRoot.Instance.UserData.HUDCash.Value = int.MaxValue;
        }
        else
        {
            GameRoot.Instance.UserData.Cash.Value += (int)convert;
            GameRoot.Instance.UserData.HUDCash.Value += (int)convert;
        }
    }

    public void SetStartTutorial()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            TpLog.LogError("input field empty!");
            return;
        }

        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            TpLog.LogError("input field string don't convert number!");
            return;
        }

        inputField.text = "";

        GameRoot.Instance.TutorialSystem.StartTutorial(convert.ToString());
    }

    public void OnClick_Hide()
    {
        GameRoot.Instance.SetCheatWindow(false);
    }

    public void AddHeroUnit()
    {

        if (string.IsNullOrEmpty(inputField.text))
        {
            TpLog.LogError("input field empty!");
            return;
        }

        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            TpLog.LogError("input field string don't convert number!");
            return;
        }

        inputField.text = "";
    }


    public void SetTicket()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            TpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            TpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.CurMode.GachaCoin.Value += (int)convert;
    }

    public void SetStarAdd()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            TpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            TpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.Starcoinvalue.Value += (int)convert;
    }


#if UNITY_EDITOR
    [UnityEditor.MenuItem("BanpoFri/ShowCheat _F3")]
    static void ShowCheat()
    {
        if (UnityEditor.EditorApplication.isPlaying)
            GameRoot.Instance.SetCheatWindow(true);
    }
#endif
}
