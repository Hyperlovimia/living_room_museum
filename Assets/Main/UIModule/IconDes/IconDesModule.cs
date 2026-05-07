using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

public class IconDesModule:ModuleBase {
	private IconDesPanel _iconDesPanel;

	private string _title;
	private string _content;
	private Texture _iconName;
	
	public override void Init() {
		base.Init();
		_iconDesPanel = new IconDesPanel();
		_iconDesPanel.SetData(_title,_content,_iconName);
		_iconDesPanel.Init(this);
		_iconDesPanel.Show(0);
	}
	public override void OnShow(float para0) {
		_iconDesPanel.Show(0);
		base.OnShow(para0);
	}

	public override void SetData(params object[] paramsObjects)
	{
		base.SetData(paramsObjects);
		_title=(string)paramsObjects[0];
		_content=(string)paramsObjects[1];
		_iconName=(Texture)paramsObjects[2];
	}
}