
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class PaperCutGameViewBase:View {
	protected RawImage _icon;
	protected Button _add;
	protected Button _sub;
	protected Text _brushSizeTxt;
	protected Button _back;
	protected Button _i1;
	protected Button _i2;
	protected Button _i3;

	protected void InitVariable() {
		_icon  = transform.Find("_raw_icon").GetComponent<RawImage>();
		_add  = transform.Find("_btn_add").GetComponent<Button>();
		_sub  = transform.Find("_btn_sub").GetComponent<Button>();
		_brushSizeTxt  = transform.Find("_txt_brushSizeTxt").GetComponent<Text>();
		_back  = transform.Find("_btn_back").GetComponent<Button>();
		_i1  = transform.Find("_btn_i1").GetComponent<Button>();
		_i2  = transform.Find("_btn_i2").GetComponent<Button>();
		_i3  = transform.Find("_btn_i3").GetComponent<Button>();
		
	}

}