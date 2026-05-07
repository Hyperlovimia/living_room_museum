using System;
using Assets.Scripts.Framework.GalaSports.Core;

using UnityEngine;

public class PauseView:PauseViewBase {
    private void Awake()
    {
        InitVariable();
        Btn(_bg,()=>ModuleManager.Instance.GoBack());
        Btn(_continueSee,()=>ModuleManager.Instance.GoBack());
        Btn(_home,()=>ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_HOME));
        Btn(_quit,Application.Quit);
    }
}