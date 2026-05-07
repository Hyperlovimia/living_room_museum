using Assets.Scripts.Framework.GalaSports.Core;

public class LoginModule : ModuleBase
{
    private LoginPanel _loginPanel;

    private LoadingPanel _loadingPanel;

    public override void Init()
    {
        base.Init();
        _loginPanel = new LoginPanel();
        _loginPanel.Init(this);
        _loginPanel.Show(0);

        _loadingPanel = new LoadingPanel();
        _loadingPanel.Init(this);
        _loadingPanel.Hide();
    }

    public override void OnShow(float para0)
    {
        _loginPanel.Show(0);
        base.OnShow(para0);
    }
}