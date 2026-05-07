using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

public class VidoFirstFrame : MonoBehaviour
{
    private void Awake()
    {
        Observable.NextFrame().Subscribe(_=>  transform.GetComponent<VideoPlayer>().Pause());
    }
}