using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
[RequireComponent(typeof(MeshCollider))]
public class DoorDo : DoBase
{
    public bool IsOpen;
    public Transform Target;
    public float closeAngle;
    public float openAngle;
    public float durTime=1;

    protected override void OnSelected(XrSelectContext context)
    {
        IsOpen = !IsOpen;
        var temp = Target == null ? transform : Target;
        if (IsOpen)
            DOTween.To(x => temp.localEulerAngles = new Vector3(0, x, 0), closeAngle, openAngle, durTime);
        else
            DOTween.To(x => temp.localEulerAngles = new Vector3(0, x, 0), openAngle, closeAngle, durTime);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
