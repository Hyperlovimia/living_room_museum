using Assets.Scripts.Framework.GalaSports.Core;

public class VideoModule:ModuleBase {
	private VideoPanel _videoPanel;

	public override void Init() {
		base.Init();
		_videoPanel = new VideoPanel();
		_videoPanel.Init(this);
		_videoPanel.Show(0);
	}
	public override void OnShow(float para0) {
		_videoPanel.Show(0);
		base.OnShow(para0);
	}

}