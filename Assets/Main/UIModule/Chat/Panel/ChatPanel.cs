
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class ChatPanel:Panel {
	private ChatController _chatController;

	public override void Init(IModule para0) {
		base.Init(para0);
		ChatView viewScript = (ChatView)InstantiateView<ChatView>("Chat/ChatView");
		RegisterView(viewScript);
		_chatController = new ChatController();
		_chatController.chatView = viewScript;
		RegisterController(_chatController);
		_chatController.Start();
	}
	public override void Show(float para0) {
		_chatController.chatView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_chatController.chatView.Hide();
		base.Hide();
		
	}

}