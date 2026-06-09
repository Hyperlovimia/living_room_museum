using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class WelcomeSequenceController : MonoBehaviour
{
    [SerializeField] private Transform playerController;
    [SerializeField] private AudioClip narrationClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float duration = 9f;
    [SerializeField] private string title = "中华起居文化";
    [TextArea(2, 8)]
    [SerializeField] private string[] captions =
    {
        "一方厅堂，承载礼序与日常。",
        "几案、椅榻、屏风与茶具，将生活方式转化为空间语言。",
        "请在展厅中点击展品，聆听讲解，并完成找一找挑战。"
    };

    private GameObject _root;
    private Text _captionText;
    private Image _progressImage;
    private AudioSource _audioSource;
    private float _startedAt;
    private bool _playing;

    public void Configure(Transform player, AudioClip audioClip)
    {
        playerController = player;
        narrationClip = audioClip;
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 0f;
        BuildUi();
        SetVisible(false);
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_playing)
        {
            return;
        }

        var elapsed = Time.time - _startedAt;
        if (_progressImage != null)
        {
            _progressImage.fillAmount = Mathf.Clamp01(elapsed / duration);
        }

        if (_captionText != null && captions != null && captions.Length > 0)
        {
            var index = Mathf.Clamp(Mathf.FloorToInt(elapsed / Mathf.Max(0.1f, duration) * captions.Length), 0, captions.Length - 1);
            _captionText.text = captions[index];
        }

        if (elapsed >= duration || WasClosePressed())
        {
            Stop();
        }
    }

    public void Play()
    {
        _startedAt = Time.time;
        _playing = true;
        SetPlayerPaused(true);
        SetVisible(true);

        if (narrationClip != null)
        {
            _audioSource.clip = narrationClip;
            _audioSource.Play();
        }
    }

    public void Stop()
    {
        _playing = false;
        _audioSource.Stop();
        SetVisible(false);
        SetPlayerPaused(false);
    }

    private void SetPlayerPaused(bool paused)
    {
        if (playerController == null)
        {
            return;
        }

        var firstPersonController = playerController.GetComponentInChildren<FirstPersonController>(true);
        if (firstPersonController != null)
        {
            firstPersonController.enabled = !paused;
        }

        var inputs = playerController.GetComponentInChildren<StarterAssetsInputs>(true);
        if (inputs != null)
        {
            inputs.cursorInputForLook = !paused;
            inputs.MoveInput(Vector2.zero);
            inputs.LookInput(Vector2.zero);
        }
    }

    private void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
        }
    }

    private void BuildUi()
    {
        var font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Arial" }, 16);
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        var canvasGo = new GameObject("WelcomeVideoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        _root = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(canvasGo.transform, false);
        var rootRect = _root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        _root.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.94f);

        var titleText = CreateText("Title", _root.transform, font, 54, FontStyle.Bold, TextAnchor.MiddleCenter);
        titleText.text = title;
        titleText.color = new Color(0.96f, 0.84f, 0.58f, 1f);
        SetRect(titleText.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(980f, 90f));

        _captionText = CreateText("Caption", _root.transform, font, 30, FontStyle.Normal, TextAnchor.MiddleCenter);
        _captionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _captionText.lineSpacing = 1.2f;
        SetRect(_captionText.rectTransform, new Vector2(0.5f, 0.48f), new Vector2(980f, 140f));

        var hintText = CreateText("Hint", _root.transform, font, 20, FontStyle.Italic, TextAnchor.MiddleCenter);
        hintText.text = "左键或 Esc 跳过";
        hintText.color = new Color(1f, 1f, 1f, 0.72f);
        SetRect(hintText.rectTransform, new Vector2(0.5f, 0.28f), new Vector2(320f, 38f));

        var progressBg = new GameObject("Progress", typeof(RectTransform), typeof(Image));
        progressBg.transform.SetParent(_root.transform, false);
        var progressRect = progressBg.GetComponent<RectTransform>();
        SetRect(progressRect, new Vector2(0.5f, 0.24f), new Vector2(480f, 8f));
        progressBg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);

        var progressFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        progressFill.transform.SetParent(progressBg.transform, false);
        var fillRect = progressFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        _progressImage = progressFill.GetComponent<Image>();
        _progressImage.color = new Color(0.94f, 0.64f, 0.22f, 1f);
        _progressImage.type = Image.Type.Filled;
        _progressImage.fillMethod = Image.FillMethod.Horizontal;
    }

    private static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = align;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 centerAnchor, Vector2 size)
    {
        rect.anchorMin = centerAnchor;
        rect.anchorMax = centerAnchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private static bool WasClosePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        var escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        return mousePressed || escapePressed;
#else
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
