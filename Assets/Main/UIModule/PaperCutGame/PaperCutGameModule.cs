using Assets.Scripts.Framework.GalaSports.Core;

public class PaperCutGameModule:ModuleBase {
	private PaperCutGamePanel _paperCutGamePanel;

	public override void Init() {
		base.Init();
		_paperCutGamePanel = new PaperCutGamePanel();
		_paperCutGamePanel.Init(this);
		_paperCutGamePanel.Show(0);
	}
	public override void OnShow(float para0) {
		_paperCutGamePanel.Show(0);
		base.OnShow(para0);
	}

}