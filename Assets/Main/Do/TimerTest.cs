using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TimerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ =>
        {
            //Debug.Log("延迟两秒");
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}