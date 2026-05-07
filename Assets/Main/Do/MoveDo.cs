using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class MoveDo : DoBase
{
    public Transform Target;
    private bool _isMove;
    public bool IsX;
    public bool IsY;
    public bool IsZ;
    public Vector3 EndPos;
    public float Time = 1;
    private Vector3 _startPos;

    private void Awake()
    {
        Target = Target == null ? this.transform : Target;
        _startPos = Target.localPosition;
    }

    private void OnMouseDown()
    {
        if (IsPointerOverUI()) return;
        _isMove = !_isMove;
        var pos = _isMove
            ? new Vector3(IsX ? EndPos.x : _startPos.x, IsY ? EndPos.y : _startPos.y, IsZ ? EndPos.z : _startPos.z)
            : _startPos;
        Target.DOLocalMove(pos, Time);
    }
}