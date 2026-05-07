using Assets.Scripts.Framework.GalaSports.Core;

public class PauseModule:ModuleBase {
	private PausePanel _pausePanel;

	public override void Init() {
		base.Init();
		_pausePanel = new PausePanel();
		_pausePanel.Init(this);
		_pausePanel.Show(0);
	}
	public override void OnShow(float para0) {
		_pausePanel.Show(0);
		base.OnShow(para0);
	}

}