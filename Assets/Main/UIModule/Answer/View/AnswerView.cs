using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Framework.GalaSports.Core;
using UnityEngine;
using UnityEngine.UI;

public class DataItem
{
    public string Title;
    public string A;
    public string B;
    public string C;
    public string D;
    public int Anser;

    public DataItem(string title, string a, string b, string c, string d, int anser)
    {
        this.Title = title;
        this.A = a;
        this.B = b;
        this.C = c;
        this.D = d;
        this.Anser = anser;
    }
}

public class AnswerView : AnswerViewBase
{
    private List<Toggle> _toggles;
    private int _select;
    private bool _currentRight = false;
    public List<DataItem> Datas = new List<DataItem>();
    private int _index = -1;

    private void Awake()
    {
        InitVariable();
        _back.onClick.AddListener(() => ModuleManager.Instance.GoBack());
        _toggles = transform.GetComponentsInChildren<Toggle>(true).ToList();
        var txt = transform.Find("Data").GetComponent<AnswerData>().Datas;
        var item = txt.Split('|').ToList();
        item.ForEach(m =>
        {
            var temp = m.Trim();
            var row = temp.Split(';');
            if (row.Length != 6) return;
            Datas.Add(new DataItem(row[0], row[1], row[2], row[3], row[4], int.Parse(row[5])));
        });

        _toggles.ForEach(m => m.onValueChanged.AddListener(v =>
        {
            _tip.gameObject.SetActive(false);
            if (v)
                _select = _toggles.IndexOf(m);
        }));
        _pull.onClick.AddListener(() =>
        {
            if (!_currentRight)
            {
                _tip.gameObject.SetActive(true);
                var d = Datas[_index];
                if (_select == d.Anser)
                {
                    _tip.text = "恭喜正确！";
                    _currentRight = true;
                    if (_index < Datas.Count - 1)
                        _pull.transform.Find("Text").GetComponent<Text>().text = "下一题";
                    else
                        _pull.transform.Find("Text").GetComponent<Text>().text = "结束，点击关闭!";
                }
                else
                    _tip.text = "选择错误！";
            }
            else
            {
                Next();
            }
        });
        StartQuestion();
    }

    public void StartQuestion()
    {
        _index = -1;
        Next();
    }

    private void Next()
    {
        _index++;
        if (_index >= Datas.Count)
        {
            ModuleManager.Instance.GoBack();
            return;
        }

        _pull.transform.Find("Text").GetComponent<Text>().text = "提交";
        _toggles.ForEach(m => m.isOn = false);
        _tip.gameObject.SetActive(false);
        _currentRight = false;
        var d = Datas[_index];
        for (var i = 0; i < 4; i++)
        {
            var t = _toggles[i];
            var dic = new Dictionary<int, string>()
            {
                { 0, d.A },
                { 1, d.B },
                { 2, d.C },
                { 3, d.D }
            };
            _title.text = d.Title;
            t.transform.Find("Label").GetComponent<Text>().text = dic[i];
        }
    }
}