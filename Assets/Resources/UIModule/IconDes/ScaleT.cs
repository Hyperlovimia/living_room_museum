using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleT : MonoBehaviour
{
    private Material _mat;

    private float _w;

    public float BrushWidth = 0.108f;

    private void Awake()
    {
        _mat = GetComponent<RawImage>().material;
        _w = GetComponent<RectTransform>().rect.width;
    }

    public float SetWidth(float w)
    {
        BrushWidth += w;
        BrushWidth = UnityEngine.Mathf.Clamp(BrushWidth, 0, 0.3f);
        return BrushWidth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mat.SetFloat("_Width", BrushWidth);
        }

        if (Input.GetMouseButton(0))
        {
            var ca = transform.GetComponentInParent<Camera>();
            var pos = Input.mousePosition;
            var worldPos = ca.ScreenToWorldPoint(pos);
            var localPos = transform.InverseTransformPoint(worldPos);
            var x = (localPos.x + _w / 2) / _w;
            var y = (localPos.y + _w / 2) / _w;
            _mat.SetFloat("_X", x);
            _mat.SetFloat("_Y", y);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _mat.SetFloat("_Width", 0);
        }
    }
}