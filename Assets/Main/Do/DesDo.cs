using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class DesDo : DoBase
{
    public string Title;
    public string Content;
    public Texture MTexture;

    protected override void OnSelected(XrSelectContext context)
    {
        var texture = GetComponent<MeshRenderer>().material.mainTexture;
        ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_ICONDES, false, false, Title, Content,
            MTexture == null ? texture : MTexture);
    }
}
