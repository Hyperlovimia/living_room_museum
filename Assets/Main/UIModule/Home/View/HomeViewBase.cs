using Assets.Scripts.Common;
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class HomeViewBase:View {
	protected Button _enterBtn;
	protected Button _desBtn;
	protected Button _exitBtn;
	protected Button _des;
	protected Button _desClose;

	protected void InitVariable() {
		_enterBtn  = transform.Find("_btn_EnterBtn").GetComponent<Button>();
		_desBtn  = transform.Find("_btn_desBtn").GetComponent<Button>();
		_exitBtn  = transform.Find("_btn_ExitBtn").GetComponent<Button>();
		_des  = transform.Find("_btn_des").GetComponent<Button>();
		_desClose  = transform.Find("_btn_des/Bg/_btn_desClose").GetComponent<Button>();
		
	}

}