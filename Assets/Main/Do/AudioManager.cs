using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource BgAudio;

    private void Awake()
    {
        if (AudioManager.Instance == null)
        {
            AudioManager.Instance = this;
            DontDestroyOnLoad(this.gameObject);
            BgAudio = transform.GetComponent<AudioSource>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void PlayBg()
    {
        BgAudio?.Play();
    }

    public void PauseBg()
    {
        BgAudio?.Pause();
    }
    
}