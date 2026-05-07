using System;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;
using UnityEngine.UI;

public class PaperCutGameView : PaperCutGameViewBase
{
    private EraseMaskThread _eraseMask;
    private int _curBrushSize = 20;

    private RawImage _tempIcon;
    private void Awake()
    {
        InitVariable();
        // _eraseMask = transform.GetComponentInChildren<EraseMaskThread>();
        // _eraseMask.brushSize = _curBrushSize;
       
        Btn(_add, () =>
        {
            _curBrushSize += 2;
            _curBrushSize = Mathf.Clamp(_curBrushSize, 2, 50);
            RefreshBrushSizeTxt();
        });
        Btn(_sub, () =>
        {
            _curBrushSize -= 2;
            _curBrushSize = Mathf.Clamp(_curBrushSize, 2, 50);
            RefreshBrushSizeTxt();
        });
        Btn(_back, () => ModuleManager.Instance.GoBack());
        Btn(_i1, () => BtnClick(_i1));
        Btn(_i2, () => BtnClick(_i2));
        Btn(_i3, () => BtnClick(_i3));
        BtnClick(_i1);
    }

    private void BtnClick(Button btn)
    {
        if (_tempIcon != null)
            GameObject.Destroy(_tempIcon.gameObject);
        var obj = Instantiate(_icon.gameObject, this.transform);
        _tempIcon = obj.transform.GetRawImage();
        _eraseMask = obj.GetComponent<EraseMaskThread>();
        _eraseMask.brushSize = _curBrushSize;
        var icon = btn.transform.GetRawImage().texture;
        _tempIcon.texture = icon;
        _eraseMask.uiTex = _tempIcon;
        _eraseMask.Init();
        obj.gameObject.Show();
        RefreshBrushSizeTxt();
    }

    private void RefreshBrushSizeTxt()
    {
        _eraseMask.brushSize = _curBrushSize;
        _brushSizeTxt.text = "笔刷大小:" + _curBrushSize;
    }
}