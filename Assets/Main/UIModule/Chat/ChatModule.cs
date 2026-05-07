using Assets.Scripts.Framework.GalaSports.Core;

public class ChatModule:ModuleBase {
	private ChatPanel _chatPanel;

	public override void Init() {
		base.Init();
		_chatPanel = new ChatPanel();
		_chatPanel.Init(this);
		_chatPanel.Show(0);
	}
	public override void OnShow(float para0) {
		_chatPanel.Show(0);
		base.OnShow(para0);
	}

}