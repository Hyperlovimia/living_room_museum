
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class LoginPanel:Panel {
	private LoginController _loginController;

	public override void Init(IModule para0) {
		base.Init(para0);
		LoginView viewScript = (LoginView)InstantiateView<LoginView>("Login/LoginView");
		RegisterView(viewScript);
		_loginController = new LoginController();
		_loginController.loginView = viewScript;
		RegisterController(_loginController);
		_loginController.Start();
	}
	public override void Show(float para0) {
		_loginController.loginView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_loginController.loginView.Hide();
		base.Hide();
		
	}

}