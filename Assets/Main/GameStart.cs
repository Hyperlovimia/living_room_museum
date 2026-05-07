using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private void Awake()
    {
        RoomLoop.StartModuleName = ModuleConfig.MODULE_HOME;
        ModuleManager.Instance.EnterModuleCb(() => Player._.Pause());
        ModuleManager.Instance.ExitAllModuleCb(() => Player._.ReStart());
        gameObject.AddComponent<GameMain>();
    }
}