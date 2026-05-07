using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using RenderSettings = UnityEngine.RenderSettings;

[RequireComponent(typeof(MeshCollider))]
public class Switch : DoBase
{
    public float OffValue = 0.5f;

    public float OnValue = 2f;
    public bool IsActive=true;

    private void Start()
    {
        RenderSettings.ambientIntensity = IsActive ? OnValue : OffValue;
    }

    private void OnMouseDown()
    {
        if (IsPointerOverUI()) return;
        IsActive = !IsActive;
        RenderSettings.ambientIntensity = IsActive ? OnValue : OffValue;
    }
}