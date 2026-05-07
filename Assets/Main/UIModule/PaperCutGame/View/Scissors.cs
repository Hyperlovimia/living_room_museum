using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scissors : MonoBehaviour
{
    private Camera _mainCa;
    private RectTransform _rt;

    public float _w;
    public float _h;
    public Transform JianDaO;

    private void Awake()
    {
        _rt = transform.GetComponent<RectTransform>();
        _mainCa = Camera.main;
        _w = _rt.rect.width / 2;
        _h = _rt.rect.height / 2;
    }

    // Update is called once per frame
    void Update()
    {
        var v = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, v, _mainCa, out var point);
        if (point.x < -_w || point.x > _w || point.y < -_h || point.y > _h)
        {
            JianDaO.gameObject.SetActive(false);
        }
        else
        {
            JianDaO.gameObject.SetActive(true);
        }

        JianDaO.transform.localPosition = new Vector3(point.x, point.y, 0);
    }
}