
using Assets.Scripts.Framework.GalaSports.Core;
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine;

public class IconDesPanel:Panel {
	private IconDesController _iconDesController;

	private string _title;
	private string _content;
	private Texture _iconName;
	public override void Init(IModule para0) {
		base.Init(para0);
		IconDesView viewScript = (IconDesView)InstantiateView<IconDesView>("IconDes/IconDesView");
		RegisterView(viewScript);
		_iconDesController = new IconDesController();
		_iconDesController.SetData(_title,_content,_iconName);
		_iconDesController.iconDesView = viewScript;
		RegisterController(_iconDesController);
		_iconDesController.Start();
	
	}
	public override void Show(float para0) {
		_iconDesController.iconDesView.Show();
		base.Show(para0);
		
	}
	public override void Hide() {
		_iconDesController.iconDesView.Hide();
		base.Hide();
		
	}

	public void SetData(string title, string c, Texture icon)
	{
		_title = title;
		_content = c;
		_iconName = icon;
	}
}