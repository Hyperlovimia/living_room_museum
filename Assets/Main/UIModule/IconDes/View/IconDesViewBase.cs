
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class IconDesViewBase:View {
	protected RawImage _icon;
	protected Button _add;
	protected Button _sub;
	protected Text _title;
	protected Text _des;
	protected Button _back;
	protected Text _xishu;

	protected void InitVariable() {
		_icon  = transform.Find("_raw_icon").GetComponent<RawImage>();
		_add  = transform.Find("_btn_add").GetComponent<Button>();
		_sub  = transform.Find("_btn_sub").GetComponent<Button>();
		_title  = transform.Find("_txt_title").GetComponent<Text>();
		_des  = transform.Find("_txt_des").GetComponent<Text>();
		_back  = transform.Find("_btn_back").GetComponent<Button>();
		_xishu  = transform.Find("_txt_xishu").GetComponent<Text>();
		
	}

}