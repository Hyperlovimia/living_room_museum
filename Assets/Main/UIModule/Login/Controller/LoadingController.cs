using System;
using Assets.Scripts.Framework.GalaSports.Core;

public class LoadingController : Controller
{
    public LoadingView loadingView;

    public override void OnMessage(ModuleMessage message)
    {
        switch (message.Name)
        {
            case ModuleMessageConst.CMD_CLICKSTARTGAME:
                loadingView.Show();
                loadingView.ShowLoadingAni(() => { GameMain.game.SwithGameLoop(GameLoopType.GameRoom); });
                break;
        }
    }
}