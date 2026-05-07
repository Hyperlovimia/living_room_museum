using System;
using System.Collections.Generic;
using Assets.Scripts.Framework.GalaSports.Core;
using DG.Tweening;

public class ChatView : ChatViewBase
{
    private List<string> _list = new List<string>();
    private int _index = -1;

    private void Awake()
    {
        InitVariable();
        _list.Add("尊敬的各位游客，大家好！");
        _list.Add("欢迎来到剪纸博物馆！我是今天的导游，很高兴有机会为大家介绍剪纸艺术的世界。");
        _list.Add("剪纸，作为中国传统手工艺之一，拥有着悠久的历史和深厚的文化底蕴。它不仅是一种艺术形式，更是中国人民对生活情感的寄托和表达。");
        _list.Add("今天，我们将深入探索剪纸的魅力，了解其历史渊源、技艺传承以及在中国文化中的重要地位。");
        _list.Add("在剪纸博物馆里，您将有机会欣赏到各种形式的剪纸作品，从传统的花鸟鱼虫到现代的创意设计，每一张作品都蕴含着匠心独运的艺术魅力。");
        _list.Add("通过这些作品，我们可以感受到中国人民对美好生活的向往和追求，也能够体味到剪纸艺术所传递的文化内涵和审美情趣。");
        _list.Add("希望您在剪纸博物馆的参观中度过愉快的时光！祝您游玩愉快，谢谢！");
        StartChat();
        _bg.onClick.AddListener(StartChat);
        Btn(_back, () => ModuleManager.Instance.GoBack());
    }

    private bool _isChat;

    public void StartChat()
    {
        if (_isChat) return;
        _isChat = true;
        _index++;
        if (_index >= _list.Count)
        {
            ModuleManager.Instance.GoBack();
            return;
        }

        _conent.text = "";
        _conent.DOText(_list[_index], 10).SetSpeedBased().SetEase(Ease.Linear).onComplete = () => _isChat = false;
    }
}