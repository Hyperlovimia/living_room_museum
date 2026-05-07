
using Assets.Scripts.Framework.GalaSports.Interfaces;
using UnityEngine.UI;
using UnityEngine;
using SuperScrollView;
using Assets.Scripts.Framework.GalaSports.Core;

public class VideoViewBase:View {
	protected GameObject _videoGo;
	protected Button _play;
	protected Button _back;
	protected Button _song;

	protected void InitVariable() {
		_videoGo  = transform.Find("_go_videoGo").gameObject;
		_play  = transform.Find("_btn_play").GetComponent<Button>();
		_back  = transform.Find("_btn_back").GetComponent<Button>();
		_song  = transform.Find("_btn_song").GetComponent<Button>();
		
	}

}