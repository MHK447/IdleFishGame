using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;


public enum IngameEventType
{
    None,
}

public sealed class BpParameter
{
    private string _key;
    private long _value;
    private double _dvalue;
    private string _strvalue;

    public BpParameter(string key, long value)
    {
        _key = key;
        _value = value;
        _dvalue = -1d;
        _strvalue = string.Empty;
    }

    public BpParameter(string key, string strvalue)
    {
        _key = key;
        _value = -1;
        _dvalue = -1d;
        _strvalue = strvalue;
    }

    public BpParameter(string key, double dvalue)
    {
        _key = key;
        _value = -1;
        _dvalue = dvalue;
        _strvalue = string.Empty;
    }

    public Parameter ConvertToFirebaseParameter()
    {
        if (_dvalue > -1d)
            return new Parameter(_key, _dvalue);
        else if (_value > -1)
            return new Parameter(_key, _value);
        else
            return new Parameter(_key, _strvalue);
    }

    public KeyValuePair<string, string> ConvertToKeyValuePair()
    {
        if (_dvalue > -1d)
            return new KeyValuePair<string, string>(_key, _dvalue.ToString());
        else if (_value > -1)
            return new KeyValuePair<string, string>(_key, _value.ToString());
        else
            return new KeyValuePair<string, string>(_key, _strvalue);
    }
}

public class BPAnalyticsProp
{
    public enum Analytics
    {
        Firebase,
        Appsflyer
    }


    public enum LogCostCashType
    {
        CardOpen = 0,
        CardActiveSlot = 1,
        LabBuyMineral,
        UnitCardOpen,
        FoodReRoll,
        WeaponReRoll,
        StartWeaponBuy,
    }


    private List<IAnalytics> analyticsList = new List<IAnalytics>() { new TpFirebaseProp()};
    public void AllEvent(IngameEventType eventType, string eventName, params object[] args)
    {
        foreach (var analytics in analyticsList)
        {
            analytics.Event(eventType, eventName, args);
        }
    }

    public void TargetEvent(Analytics analytics, IngameEventType eventType, string eventName, params object[] args)
    {
        analyticsList[(int)analytics].Event(eventType, eventName, args);
    }

    public void InAppPurchaseEvent(params object[] args)
    {
        foreach (var analytics in analyticsList)
        {
            analytics.InAppPurchaseEvent(args);
        }
    }
}

public class TpFirebaseProp : IAnalytics
{
    public void Event(IngameEventType eventType, string eventName, params object[] args)
    {
#if BP_LOG
        return;
#endif

        if (args.Length > 0)
        {
            List<BpParameter> tpParams = args[0] as List<BpParameter>;
            List<Parameter> parameters = new List<Parameter>();

            if (tpParams != null)
            {
                foreach (var pm in tpParams)
                {
                    parameters.Add(pm.ConvertToFirebaseParameter());
                }
            }

#if BP_LOG
            Debug.Log("fb log ===" + eventName);
#endif
            FirebaseAnalytics.LogEvent(eventName, parameters.ToArray());
        }
        else
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
    }


    public void InAppPurchaseEvent(params object[] args)
    {
        string currency = args[0] as string;
        string id = args[1] as string;
        string price = args[2] as string;
        string order_id = args[3] as string;
       
    }
}


interface IAnalytics
{
    void Event(IngameEventType eventType, string eventName, params object[] args);
    void InAppPurchaseEvent(params object[] args);
}