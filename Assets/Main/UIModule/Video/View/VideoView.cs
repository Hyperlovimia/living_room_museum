using System;
using Assets.Scripts.Framework.GalaSports.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoView : VideoViewBase
{
    private bool _isPlay = true;
    private VideoPlayer _videoPlayer;

    private Slider _videoSlider;

    private double _totalTime;

    private Slider _songSlider;

    private Tweener _t;

    private bool _vShow;

    private void Awake()
    {
        InitVariable();
        _videoPlayer = _videoGo.GetComponent<VideoPlayer>();
        _totalTime = _videoPlayer.clip.length;
        _videoSlider = transform.Find("videoSlider").GetComponent<Slider>();
        _songSlider = transform.Find("voloumSlider").GetComponent<Slider>();
        _videoSlider.onValueChanged.AddListener(m =>
        {
            _time = 0;
            _videoPlayer.time = _videoSlider.value * _videoPlayer.clip.length;
            //_t?.Kill();
            //_t = DOTween.To(x => _videoSlider.value = x, _videoSlider.value, 1,
            //  (1 - _videoSlider.value) * (int)_totalTime);
        });
        _songSlider.onValueChanged.AddListener(m =>
        {
            //_videoPlayer.tra
            _videoGo.GetComponent<AudioSource>().volume = m;
        });

        // _t = DOTween.To(x => _videoSlider.value = x, 0, 1, (int)_totalTime);

        Btn(_play, () =>
        {
            _isPlay = !_isPlay;
            if (_isPlay)
                _videoPlayer.Play();
            else
                _videoPlayer.Pause();
            _play.transform.GetChild(0).gameObject.SetActive(_isPlay);
            _play.transform.GetChild(1).gameObject.SetActive(!_isPlay);
        });

        Btn(_song, () =>
        {
            _vShow = !_vShow;
            _songSlider.gameObject.SetActive(_vShow);
        });
        Btn(_back, () => ModuleManager.Instance.GoBack());
    }

    private float _time;

    private void LateUpdate()
    {
        _time += Time.deltaTime;
        if (_time >= 3f)
        {
            Debug.LogError("gg");
            _videoSlider.value = (float)_videoPlayer.time / (float)_totalTime;
            _time -= 3f;
        }
    }
}