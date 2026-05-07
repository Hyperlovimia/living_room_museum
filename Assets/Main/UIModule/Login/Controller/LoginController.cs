using Assets.Scripts.Framework.GalaSports.Core;

public class LoginController : Controller
{
    public LoginView loginView;

    public override void OnMessage(ModuleMessage message)
    {
        switch (message.Name)
        {
            case ModuleMessageConst.CMD_CLICKSTARTGAME:
                loginView.Show();
                break;
        }
    }
}