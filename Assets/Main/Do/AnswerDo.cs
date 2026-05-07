using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

public class AnswerDo : DoBase
{
    
    private void OnMouseDown()
    {
        if (!IsPointerOverUI())
            ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_ANSWER);
    }
}