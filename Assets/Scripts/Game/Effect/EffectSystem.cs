using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using BanpoFri;
[AttributeUsage(AttributeTargets.Class)]
public class EffectPathAttribute : Attribute
{
    private string path;
    private bool ui;
    private bool worldUI;
    public EffectPathAttribute(string name, bool _ui = false, bool _worldUI = false)
    {
        path = name;
        ui = _ui;
        worldUI = _worldUI;
    }
    public string Path
    {
        get
        {
            return path;
        }
    }

    public bool InUI
    {
        get
        {
            return ui;
        }
    }

    public bool InWorldUI
    {
        get
        {
            return worldUI;
        }
    }
}

public class EffectSystem 
{
    private Dictionary<Type, Effect> EffectDic = new Dictionary<Type, Effect>();
    private Dictionary<Type, List<Effect>> MultiEffectDic = new Dictionary<Type, List<Effect>>();
    private System.Action ForceCollect = null;

    public void Play<T>(Vector3 worldPos, Action<T> OnLoad = null, Transform followTrans = null) where T : Effect
    {
        var effectType = typeof(T);
        if(EffectDic.ContainsKey(effectType))
        {
            if(EffectDic[effectType] != null)
            {
                if (!EffectDic[effectType].gameObject.activeSelf)
                    EffectDic[effectType].gameObject.SetActive(true);
                EffectDic[effectType].Play(worldPos, followTrans);
            }

            OnLoad?.Invoke(EffectDic[effectType] as T);
        }
        else
        {
            var attrs = Attribute.GetCustomAttributes(effectType);
            foreach( var attr in attrs)
            {
                if(attr is EffectPathAttribute)
                {
                    var effectPath = (EffectPathAttribute) attr;
                    EffectDic.Add(effectType, null);
                    Transform parent = null;
                    if(effectPath.InUI)
                        parent = GameRoot.Instance.UISystem.UIRootT.transform;
                    else if(effectPath.InWorldUI)
                        parent = GameRoot.Instance.UISystem.WorldCanvas.transform;
                    else
                        parent = GameRoot.Instance.transform;
                    var handle = Addressables.InstantiateAsync(effectPath.Path, worldPos, Quaternion.identity, parent);
                    handle.Completed += (obj) => {
                        var inst = obj.Result;

                        var effect = inst.GetComponent<T>();                            
                        EffectDic[effectType] = effect;
                        effect.Play(worldPos, followTrans);
                        OnLoad?.Invoke(effect);
                    };
                }
            }
        }
    }

    public void MultiPlay<T>(Vector3 worldPos, Action<T> OnLoad = null, Transform followTrans = null) where T : Effect
    {
        var effectType = typeof(T);
        if(MultiEffectDic.ContainsKey(effectType) && MultiEffectDic[effectType].Count > 0)
        {
            var effect = MultiEffectDic[effectType][0];
            MultiEffectDic[effectType].RemoveAt(0);

            if(effect != null)
            {
                effect.Play(worldPos, followTrans);
                ProjectUtility.SetActiveCheck(effect.gameObject, true);
                OnLoad?.Invoke(effect as T);
                if(ForceCollect == null)
                    ForceCollect = effect.OnForceCollect;
                else                
                    ForceCollect += effect.OnForceCollect;
                
            }
        }
        else
        {
            var attrs = Attribute.GetCustomAttributes(effectType);
            foreach( var attr in attrs)
            {
                if(attr is EffectPathAttribute)
                {
                    var effectPath = (EffectPathAttribute) attr;
                    if(!MultiEffectDic.ContainsKey(effectType))
                        MultiEffectDic.Add(effectType, new List<Effect>());

                    Transform parent = null;
                    if(effectPath.InUI)
                        parent = GameRoot.Instance.UISystem.UIRootT.transform;
                    else if(effectPath.InWorldUI)
                        parent = GameRoot.Instance.UISystem.WorldCanvas.transform;
                    else
                        parent = GameRoot.Instance.transform;

                    var handle = Addressables.InstantiateAsync(effectPath.Path, worldPos, Quaternion.identity, parent, false);
                    handle.Completed += (obj) => {
                        var inst = obj.Result;
                        var effect = inst.GetComponent<T>();
                        effect.Play(worldPos, followTrans);
                        ProjectUtility.SetActiveCheck(effect.gameObject, true);
                        OnLoad?.Invoke(effect);
                        if(ForceCollect == null)
                            ForceCollect = effect.OnForceCollect;
                        else                
                            ForceCollect += effect.OnForceCollect;
                    };
                }
            }
        }        
    }

    public void Clear()
    {
        ForceCollect?.Invoke();
        ForceCollect = null;
        foreach(var eff in EffectDic)
        {
            if(eff.Value != null)            
                if(!Addressables.ReleaseInstance(eff.Value.gameObject))
                    GameObject.Destroy(eff.Value.gameObject);
        }
        EffectDic.Clear();
        foreach(var effList in MultiEffectDic)
        {
            if(effList.Value != null)
            {
                foreach(var eff in effList.Value)
                    if(eff != null)
                        if(!Addressables.ReleaseInstance(eff.gameObject))
                            GameObject.Destroy(eff.gameObject);
                effList.Value.Clear();
            }   
        }
        MultiEffectDic.Clear();
    }

    public void ReturnMultiEffect<T>(T target) where T : Effect
    {
        if(target == null)
            return;

        if(ForceCollect != null)
            ForceCollect -= target.OnForceCollect;

        ProjectUtility.SetActiveCheck(target.gameObject, false);
        var effectType = typeof(T);
        if(MultiEffectDic.ContainsKey(effectType))
        {
            MultiEffectDic[effectType].Add(target);
        }
        else
        {
            MultiEffectDic.Add(effectType, new List<Effect>() { target });
        }
    }

    public void ReturnMultiEffect(Effect target)
    {
        if(target == null)
            return;

        if(ForceCollect != null)
            ForceCollect -= target.OnForceCollect;
        ProjectUtility.SetActiveCheck(target.gameObject, false);
        var effectType = target.GetType();
        if(MultiEffectDic.ContainsKey(effectType))
        {
            MultiEffectDic[effectType].Add(target);
        }
        else
        {
            MultiEffectDic.Add(effectType, new List<Effect>() { target });
        }
    }

    public void Stop<T>() where T : Effect
    {
        var effectType = typeof(T);
        if(EffectDic.ContainsKey(effectType))
        {
            if(EffectDic[effectType] != null)
                EffectDic[effectType].Stop();
        }
    }

    public T Get<T>() where T : Effect
    {
        var effectType = typeof(T);
        if(EffectDic.ContainsKey(effectType))
        {
            return EffectDic[effectType] as T;
        }
        return null;
    } 
}
