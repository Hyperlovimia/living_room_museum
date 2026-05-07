using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesPanel : MonoBehaviour
{
    public GameObject Panel;

    public static DesPanel Instance;

    private Text _title;
    private Text _content;
    private void Awake()
    {
        Instance = this;
    }

    public void ShowPanel(string title, string content)
    {
        Panel.gameObject.SetActive(true);
        _title = Panel.transform.Find("title").GetComponent<Text>();
        _content = Panel.transform.Find("content").GetComponent<Text>();
        _title.text = title;
        _content.text = content;
    }
}
