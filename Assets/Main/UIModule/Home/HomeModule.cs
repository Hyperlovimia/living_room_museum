using Assets.Scripts.Framework.GalaSports.Core;

public class HomeModule:ModuleBase {
	private HomePanel _homePanel;

	public override void Init() {
		base.Init();
		_homePanel = new HomePanel();
		_homePanel.Init(this);
		_homePanel.Show(0);
	}
	public override void OnShow(float para0) {
		_homePanel.Show(0);
		base.OnShow(para0);
	}

}