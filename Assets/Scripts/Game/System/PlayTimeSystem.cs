using UnityEngine;
using System.Collections;
using System;
using System.Globalization;
using UnityEngine.Networking;
using UniRx;
using UnityEditor;


public class PlayTimeSystem
{

    public IReactiveProperty<int> RemainTimeProperty = new ReactiveProperty<int>();

    private float deltaTime = 0f;


    public IObservable<float> CreateCountDownObservable(float countTime) =>
        Observable
            .Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            .Select(x => (countTime - x))
            .TakeWhile(x => x > 0);
    

    public void Update()
    {
        if (deltaTime < 1f)
        {
            deltaTime += Time.deltaTime;
            return;
        }

        deltaTime -= 1f;

        RemainTimeProperty.Value += 1;
    }

}
