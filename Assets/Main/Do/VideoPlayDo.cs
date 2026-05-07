using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayDo : DoBase
{
    private VideoPlayer _video;


    private void Awake()
    {
        _video = transform.GetComponent<VideoPlayer>();
    }

    public void Pause()
    {
        _video.Pause();
    }

    private void OnMouseDown()
    {
        if(IsPointerOverUI()) return;
        if (!_video.isPlaying)
        {
            GameObject.FindObjectsOfType<VideoPlayDo>().ToList().ForEach(m => m.Pause());
            _video.Play();
            AudioManager.Instance?.PauseBg();
        }
        else
        {
            _video.Pause();
            AudioManager.Instance?.PlayBg();
        }
    }
    
}