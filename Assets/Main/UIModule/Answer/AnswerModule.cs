using Assets.Scripts.Framework.GalaSports.Core;

public class AnswerModule:ModuleBase {
	private AnswerPanel _answerPanel;

	public override void Init() {
		base.Init();
		_answerPanel = new AnswerPanel();
		_answerPanel.Init(this);
		_answerPanel.Show(0);
	}
	public override void OnShow(float para0) {
		_answerPanel.Show(0);
		base.OnShow(para0);
	}

}