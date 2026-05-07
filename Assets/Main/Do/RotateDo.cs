using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class RotateDo : DoBase
{
    private Tweener _t;
    

    private void OnMouseDown()
    {
        if (!IsPointerOverUI())
        {
            if (_t != null)
            {
                _t.Kill();
                _t = null;
            }
            else
            {
                var angle = transform.eulerAngles;
                _t = transform.DORotate(new Vector3(0, angle.y + 360, 0), 3, RotateMode.FastBeyond360).SetEase(Ease.Linear)
                    .SetLoops(-1);
            }
        }
    }
}