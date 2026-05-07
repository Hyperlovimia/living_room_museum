
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class PauseViewBase:View {
	protected Button _bg;
	protected Button _continueSee;
	protected Button _home;
	protected Button _quit;
	protected Slider _sliderV;

	protected void InitVariable() {
		_bg  = transform.Find("_btn_Bg").GetComponent<Button>();
		_continueSee  = transform.Find("Image/_btn_continueSee").GetComponent<Button>();
		_home  = transform.Find("Image/_btn_home").GetComponent<Button>();
		_quit  = transform.Find("Image/_btn_quit").GetComponent<Button>();
		_sliderV  = transform.Find("Image/_Slider_sliderV").GetComponent<Slider>();
		
	}

}