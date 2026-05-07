using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class ChatViewBase:View {
	protected Button _bg;
	protected Text _conent;
	protected Button _back;

	protected void InitVariable() {
		_bg  = transform.Find("_btn_bg").GetComponent<Button>();
		_conent  = transform.Find("_btn_bg/_txt_conent").GetComponent<Text>();
		_back  = transform.Find("_btn_back").GetComponent<Button>();
		
	}

}