using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Firestore;

public class BpFireBaseDataProp
{

    public void Init()
    {
        var firestore = FirebaseFirestore.DefaultInstance;
        firestore.Settings.PersistenceEnabled = false;
    }
}
