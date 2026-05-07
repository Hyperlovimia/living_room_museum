using System;
using Assets.Scripts.Framework.GalaSports.Core;

using UnityEngine;

public class HomeView : HomeViewBase
{
    private void Awake()
    {
        InitVariable();
        Btn(_desBtn, () => _des.gameObject.Show());
        Btn(_enterBtn, () => { ModuleManager.Instance.GoBack(); });
        Btn(_exitBtn, Application.Quit);
        Btn(_desClose, () => _des.gameObject.Hide());
       // Btn(_videoBtn, () => ModuleManager.Instance.EnterModule(ModuleConfig.MODULE_VIDEO, false));
    }
}