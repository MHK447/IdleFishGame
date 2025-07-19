using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BanpoFri;
using Firebase.Extensions;
using Firebase.Crashlytics;
using Firebase.Analytics;

public class PluginSystem
{
    public BpFireBaseDataProp DataProp = new BpFireBaseDataProp();

    public void OnApplicationPause(bool value)
    {
        if (value)
        {
            if (GameRoot.Instance == null) Debug.Log("GameRoot is Null");
            if (GameRoot.Instance.UserData == null) Debug.Log("UserData is Null");
            if (GameRoot.Instance.UserData.CurMode == null)
            {
                Debug.Log("CurMode is Null");
                return;
            }

            GameRoot.Instance.UserData.CurMode.LastLoginTime = TimeSystem.GetCurTime();
            GameRoot.Instance.UserData.Save(true);
        }
    }
        

    public void Init()
    {
#if !UNITY_EDITOR
        DataProp.Init();

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Crashlytics will use the DefaultInstance, as well;
                // this ensures that Crashlytics is initialized.
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here for indicating that your project is ready to use Firebase.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
#endif
    }
}