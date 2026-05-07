
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class VideoPanel:Panel {
	private VideoController _videoController;

	public override void Init(IModule para0) {
		base.Init(para0);
		VideoView viewScript = (VideoView)InstantiateView<VideoView>("Video/VideoView");
		RegisterView(viewScript);
		_videoController = new VideoController();
		_videoController.videoView = viewScript;
		RegisterController(_videoController);
		_videoController.Start();
	}
	public override void Show(float para0) {
		_videoController.videoView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_videoController.videoView.Hide();
		base.Hide();
		
	}

}