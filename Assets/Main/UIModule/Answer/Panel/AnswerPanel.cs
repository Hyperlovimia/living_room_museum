
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;

public class AnswerPanel:Panel {
	private AnswerController _answerController;

	public override void Init(IModule para0) {
		base.Init(para0);
		AnswerView viewScript = (AnswerView)InstantiateView<AnswerView>("Answer/AnswerView");
		RegisterView(viewScript);
		_answerController = new AnswerController();
		_answerController.answerView = viewScript;
		RegisterController(_answerController);
		_answerController.Start();
	}
	public override void Show(float para0) {
		_answerController.answerView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_answerController.answerView.Hide();
		base.Hide();
		
	}

}