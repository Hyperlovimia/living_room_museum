
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class LoadingPanel:Panel {
	private LoadingController _loadingController;

	public override void Init(IModule para0) {
		base.Init(para0);
		LoadingView viewScript = (LoadingView)InstantiateView<LoadingView>("Login/LoadingView");
		RegisterView(viewScript);
		_loadingController = new LoadingController();
		_loadingController.loadingView = viewScript;
		RegisterController(_loadingController);
		_loadingController.Start();
	}
	public override void Show(float para0) {
		_loadingController.loadingView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_loadingController.loadingView.Hide();
		base.Hide();
		
	}

}