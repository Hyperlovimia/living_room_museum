using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;

public class IconDesController:Controller {
	public IconDesView iconDesView;
	private string _title;
	private string _content;
	private Texture _iconName;
	public override void Start()
	{
		iconDesView.SetData(_title,_content,_iconName);
	}

	public void SetData(string title, string c, Texture icon)
	{
		_title = title;
		_content = c;
		_iconName = icon;
	}
}