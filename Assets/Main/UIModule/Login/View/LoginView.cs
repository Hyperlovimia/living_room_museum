using System;
using Assets.Scripts.Framework.GalaSports.Core;

public class LoginView : LoginViewBase
{
    private void Awake()
    {
        transform.GetButton("Start").onClick.AddListener(() =>
        {
            SendMessage(new ModuleMessage(ModuleMessageConst.CMD_CLICKSTARTGAME));
        });
    }
}