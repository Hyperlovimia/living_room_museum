using System;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

public class IconDesView : IconDesViewBase
{
    private ScaleT _scaleT;

    private void Awake()
    {
        InitVariable();
        Btn(_back, () => ModuleManager.Instance.GoBack());
        _scaleT = transform.GetComponentInChildren<ScaleT>();
       // Btn(_add, ()=> _xishu.text="放大镜系数："+_scaleT.SetWidth(0.02f));
       // Btn(_sub, ()=> _xishu.text="放大镜系数："+_scaleT.SetWidth(-0.02f));
       // _xishu.text = "放大镜系数：" + _scaleT.BrushWidth;
    }

    public void SetData(string title, string c, Texture icon)
    {
        _title.text = title;
        _des.text = c;
        _icon.texture = icon;
    }
}