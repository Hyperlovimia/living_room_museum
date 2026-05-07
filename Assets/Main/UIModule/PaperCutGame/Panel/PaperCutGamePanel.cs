
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class PaperCutGamePanel:Panel {
	private PaperCutGameController _paperCutGameController;

	public override void Init(IModule para0) {
		base.Init(para0);
		PaperCutGameView viewScript = (PaperCutGameView)InstantiateView<PaperCutGameView>("PaperCutGame/PaperCutGameView");
		RegisterView(viewScript);
		_paperCutGameController = new PaperCutGameController();
		_paperCutGameController.paperCutGameView = viewScript;
		RegisterController(_paperCutGameController);
		_paperCutGameController.Start();
	}
	public override void Show(float para0) {
		_paperCutGameController.paperCutGameView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_paperCutGameController.paperCutGameView.Hide();
		base.Hide();
		
	}

}