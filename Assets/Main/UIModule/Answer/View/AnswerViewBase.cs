
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class AnswerViewBase:View {
	protected Button _pull;
	protected Text _title;
	protected Text _tip;
	protected Button _back;

	protected void InitVariable() {
		_pull  = transform.Find("_btn_pull").GetComponent<Button>();
		_title  = transform.Find("_txt_title").GetComponent<Text>();
		_tip  = transform.Find("_txt_tip").GetComponent<Text>();
		_back  = transform.Find("_btn_back").GetComponent<Button>();
		
	}

}