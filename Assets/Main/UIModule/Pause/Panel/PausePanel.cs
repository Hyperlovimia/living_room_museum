
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class PausePanel:Panel {
	private PauseController _pauseController;

	public override void Init(IModule para0) {
		base.Init(para0);
		PauseView viewScript = (PauseView)InstantiateView<PauseView>("Pause/PauseView");
		RegisterView(viewScript);
		_pauseController = new PauseController();
		_pauseController.pauseView = viewScript;
		RegisterController(_pauseController);
		_pauseController.Start();
	}
	public override void Show(float para0) {
		_pauseController.pauseView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_pauseController.pauseView.Hide();
		base.Hide();
		
	}

}