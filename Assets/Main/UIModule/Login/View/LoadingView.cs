using System;
using Assets.Scripts.Framework.GalaSports.Core;
using DG.Tweening;
using UnityEngine.UI;

public class LoadingView : LoadingViewBase
{
    private Slider _slider;

    private void Awake()
    {
        _slider = transform.GetSlider("Slider");
    }

    public void ShowLoadingAni(Action cb)
    {
        DOTween.To(m => _slider.value = m, 0, 1, 3)
            .onComplete = ()=>cb?.Invoke();
    }
}