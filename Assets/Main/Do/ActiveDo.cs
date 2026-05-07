using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshCollider))]
public class ActiveDo : DoBase
{
    public bool IsActive;

    public Transform Target;

    private void Awake()
    {
        Target.gameObject.SetActive(IsActive);
    }

    private void OnMouseDown()
    {
        if (IsPointerOverUI()) return;
        IsActive = !IsActive;
        Target.gameObject.SetActive(IsActive);
    }
    
}