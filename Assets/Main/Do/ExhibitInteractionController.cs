using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class ExhibitInteractionController : MonoBehaviour
{
    [SerializeField] private float interactDistance = 10f;
    [SerializeField] private Vector2 panelSize = new Vector2(760f, 460f);
    [SerializeField] private Vector2 imageSize = new Vector2(240f, 150f);
    [SerializeField] private Vector2 videoSize = new Vector2(240f, 135f);
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color panelColor = new Color(0.1f, 0.09f, 0.08f, 0.94f);

    private Camera _mainCamera;
    private FirstPersonController _firstPersonController;
    private StarterAssetsInputs _starterAssetsInputs;
    private AudioSource _narrationAudioSource;
    private AudioSource _videoAudioSource;
    private VideoPlayer _videoPlayer;
    private RenderTexture _videoRenderTexture;

    private GameObject _overlayRoot;
    private GameObject _crosshairRoot;
    private GameObject _videoFrameRoot;
    private RawImage _displayImage;
    private RawImage _videoImage;
    private Text _titleText;
    private Text _descriptionText;
    private Text _hintText;
    private Text _videoHintText;

    private bool _isPanelOpen;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            _mainCamera = FindFirstObjectByType<Camera>();
        }

        _firstPersonController = GetComponentInChildren<FirstPersonController>(true);
        _starterAssetsInputs = GetComponentInChildren<StarterAssetsInputs>(true);
        _narrationAudioSource = GetComponent<AudioSource>();
        if (_narrationAudioSource == null)
        {
            _narrationAudioSource = gameObject.AddComponent<AudioSource>();
        }

        _narrationAudioSource.playOnAwake = false;
        _narrationAudioSource.loop = false;
        _narrationAudioSource.spatialBlend = 0f;

        _videoAudioSource = gameObject.AddComponent<AudioSource>();
        _videoAudioSource.playOnAwake = false;
        _videoAudioSource.loop = false;
        _videoAudioSource.spatialBlend = 0f;

        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
        _videoPlayer.playOnAwake = false;
        _videoPlayer.waitForFirstFrame = true;
        _videoPlayer.isLooping = true;
        _videoPlayer.source = VideoSource.VideoClip;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _videoPlayer.controlledAudioTrackCount = 1;
        _videoPlayer.EnableAudioTrack(0, true);
        _videoPlayer.SetTargetAudioSource(0, _videoAudioSource);
        _videoPlayer.prepareCompleted += OnVideoPrepared;
        _videoPlayer.errorReceived += OnVideoError;

        BuildUi();
        SetPanelVisible(false);
    }

    private void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.errorReceived -= OnVideoError;
        }

        ReleaseVideoTexture();
    }

    private void Update()
    {
        if (_isPanelOpen)
        {
            if (WasClosePressed())
            {
                ClosePanel();
            }

            return;
        }

        if (WasInteractPressed())
        {
            TryOpenExhibit();
        }
    }

    private void TryOpenExhibit()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        if (_mainCamera == null)
        {
            return;
        }

        var ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out var hit, interactDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        var exhibitInfo = hit.collider.GetComponentInParent<ExhibitInfo>();
        if (exhibitInfo == null)
        {
            return;
        }

        OpenPanel(exhibitInfo);
    }

    private void OpenPanel(ExhibitInfo exhibitInfo)
    {
        _isPanelOpen = true;

        _titleText.text = exhibitInfo.ExhibitTitle;
        _descriptionText.text = exhibitInfo.Description;
        _displayImage.texture = exhibitInfo.DisplayTexture;
        _displayImage.color = _displayImage.texture == null ? new Color(1f, 1f, 1f, 0f) : Color.white;
        _hintText.text = "Left Click or Esc to close";

        if (_firstPersonController != null)
        {
            _firstPersonController.enabled = false;
        }

        if (_starterAssetsInputs != null)
        {
            _starterAssetsInputs.cursorLocked = true;
            _starterAssetsInputs.cursorInputForLook = false;
            _starterAssetsInputs.MoveInput(Vector2.zero);
            _starterAssetsInputs.LookInput(Vector2.zero);
        }

        if (exhibitInfo.NarrationClip != null)
        {
            _narrationAudioSource.clip = exhibitInfo.NarrationClip;
            _narrationAudioSource.Play();
        }
        else
        {
            _narrationAudioSource.Stop();
            _narrationAudioSource.clip = null;
        }

        ConfigureVideo(exhibitInfo);
        SetPanelVisible(true);
    }

    private void ClosePanel()
    {
        _isPanelOpen = false;
        _narrationAudioSource.Stop();
        _narrationAudioSource.clip = null;
        StopVideoPlayback();

        if (_firstPersonController != null)
        {
            _firstPersonController.enabled = true;
        }

        if (_starterAssetsInputs != null)
        {
            _starterAssetsInputs.cursorLocked = true;
            _starterAssetsInputs.cursorInputForLook = true;
            _starterAssetsInputs.MoveInput(Vector2.zero);
            _starterAssetsInputs.LookInput(Vector2.zero);
        }

        SetPanelVisible(false);
    }

    private void SetPanelVisible(bool visible)
    {
        if (_overlayRoot != null)
        {
            _overlayRoot.SetActive(visible);
        }

        if (_crosshairRoot != null)
        {
            _crosshairRoot.SetActive(!visible);
        }
    }

    private void ConfigureVideo(ExhibitInfo exhibitInfo)
    {
        StopVideoPlayback();

        var videoClip = exhibitInfo.ExhibitVideo;
        var hasVideo = videoClip != null;

        if (_videoFrameRoot != null)
        {
            _videoFrameRoot.SetActive(hasVideo);
        }

        if (_videoHintText != null)
        {
            _videoHintText.text = hasVideo ? "Preparing video..." : string.Empty;
        }

        if (!hasVideo)
        {
            if (_videoImage != null)
            {
                _videoImage.texture = null;
                _videoImage.color = new Color(1f, 1f, 1f, 0f);
            }

            return;
        }

        EnsureVideoTexture();
        _videoPlayer.Stop();
        _videoPlayer.clip = videoClip;
        _videoPlayer.isLooping = exhibitInfo.LoopVideo;
        _videoPlayer.targetTexture = _videoRenderTexture;

        if (_videoImage != null)
        {
            _videoImage.texture = _videoRenderTexture;
            _videoImage.color = Color.white;
        }

        _videoPlayer.Prepare();
    }

    private void EnsureVideoTexture()
    {
        if (_videoRenderTexture != null)
        {
            return;
        }

        _videoRenderTexture = new RenderTexture(1024, 576, 0, RenderTextureFormat.ARGB32);
        _videoRenderTexture.Create();
    }

    private void ReleaseVideoTexture()
    {
        if (_videoRenderTexture == null)
        {
            return;
        }

        _videoRenderTexture.Release();
        Destroy(_videoRenderTexture);
        _videoRenderTexture = null;
    }

    private void StopVideoPlayback()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
            _videoPlayer.clip = null;
            _videoPlayer.targetTexture = null;
        }

        if (_videoAudioSource != null)
        {
            _videoAudioSource.Stop();
            _videoAudioSource.clip = null;
        }

        if (_videoImage != null)
        {
            _videoImage.texture = null;
            _videoImage.color = new Color(1f, 1f, 1f, 0f);
        }

        if (_videoHintText != null)
        {
            _videoHintText.text = string.Empty;
        }
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        if (!_isPanelOpen || player.clip == null)
        {
            return;
        }

        if (_videoHintText != null)
        {
            _videoHintText.text = string.Empty;
        }

        player.Play();
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        if (_videoHintText != null)
        {
            _videoHintText.text = "Video unavailable";
        }

        Debug.LogWarning($"Failed to prepare exhibit video: {message}", this);
    }

    private void BuildUi()
    {
        var font = LoadFont();

        var canvasRoot = new GameObject("ExhibitInfoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasRoot.transform.SetParent(transform, false);

        var canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var canvasScaler = canvasRoot.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        _overlayRoot = CreateUiNode("Overlay", canvasRoot.transform);
        var overlayRect = _overlayRoot.GetComponent<RectTransform>();
        StretchToFullScreen(overlayRect);

        var overlayImage = _overlayRoot.AddComponent<Image>();
        overlayImage.color = overlayColor;

        var panel = CreateUiNode("Panel", _overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = panelSize;

        var panelImage = panel.AddComponent<Image>();
        panelImage.color = panelColor;

        _titleText = CreateText("Title", panel.transform, font, 32, FontStyle.Bold, TextAnchor.UpperLeft);
        var titleRect = _titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(28f, -24f);
        titleRect.sizeDelta = new Vector2(-56f, 42f);

        var imageFrame = CreateUiNode("ImageFrame", panel.transform);
        var imageFrameRect = imageFrame.GetComponent<RectTransform>();
        imageFrameRect.anchorMin = new Vector2(0f, 1f);
        imageFrameRect.anchorMax = new Vector2(0f, 1f);
        imageFrameRect.pivot = new Vector2(0f, 1f);
        imageFrameRect.anchoredPosition = new Vector2(28f, -82f);
        imageFrameRect.sizeDelta = imageSize;

        var imageFrameGraphic = imageFrame.AddComponent<Image>();
        imageFrameGraphic.color = new Color(1f, 1f, 1f, 0.08f);
        imageFrameGraphic.raycastTarget = false;

        var rawImageNode = CreateUiNode("Image", imageFrame.transform);
        var rawImageRect = rawImageNode.GetComponent<RectTransform>();
        StretchWithPadding(rawImageRect, 8f);
        _displayImage = rawImageNode.AddComponent<RawImage>();
        _displayImage.raycastTarget = false;

        _videoFrameRoot = CreateUiNode("VideoFrame", panel.transform);
        var videoFrameRect = _videoFrameRoot.GetComponent<RectTransform>();
        videoFrameRect.anchorMin = new Vector2(0f, 1f);
        videoFrameRect.anchorMax = new Vector2(0f, 1f);
        videoFrameRect.pivot = new Vector2(0f, 1f);
        videoFrameRect.anchoredPosition = new Vector2(28f, -250f);
        videoFrameRect.sizeDelta = videoSize;

        var videoFrameImage = _videoFrameRoot.AddComponent<Image>();
        videoFrameImage.color = new Color(1f, 1f, 1f, 0.08f);
        videoFrameImage.raycastTarget = false;

        var videoRawImageNode = CreateUiNode("Video", _videoFrameRoot.transform);
        var videoRawImageRect = videoRawImageNode.GetComponent<RectTransform>();
        StretchWithPadding(videoRawImageRect, 8f);
        _videoImage = videoRawImageNode.AddComponent<RawImage>();
        _videoImage.color = new Color(1f, 1f, 1f, 0f);
        _videoImage.raycastTarget = false;

        _videoHintText = CreateText("VideoHint", _videoFrameRoot.transform, font, 16, FontStyle.Italic, TextAnchor.MiddleCenter);
        var videoHintRect = _videoHintText.rectTransform;
        StretchWithPadding(videoHintRect, 8f);
        _videoHintText.color = new Color(1f, 1f, 1f, 0.75f);
        _videoHintText.text = string.Empty;

        _descriptionText = CreateText("Description", panel.transform, font, 22, FontStyle.Normal, TextAnchor.UpperLeft);
        var descriptionRect = _descriptionText.rectTransform;
        descriptionRect.anchorMin = new Vector2(0f, 0f);
        descriptionRect.anchorMax = new Vector2(1f, 1f);
        descriptionRect.pivot = new Vector2(0f, 1f);
        descriptionRect.offsetMin = new Vector2(300f, 64f);
        descriptionRect.offsetMax = new Vector2(-28f, -82f);
        _descriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _descriptionText.verticalOverflow = VerticalWrapMode.Overflow;
        _descriptionText.lineSpacing = 1.2f;

        _hintText = CreateText("Hint", panel.transform, font, 18, FontStyle.Italic, TextAnchor.LowerRight);
        var hintRect = _hintText.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(1f, 0f);
        hintRect.anchoredPosition = new Vector2(-28f, 18f);
        hintRect.sizeDelta = new Vector2(-56f, 28f);
        _hintText.color = new Color(1f, 1f, 1f, 0.8f);

        _crosshairRoot = CreateUiNode("Crosshair", canvasRoot.transform);
        var crosshairRect = _crosshairRoot.GetComponent<RectTransform>();
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.pivot = new Vector2(0.5f, 0.5f);
        crosshairRect.anchoredPosition = Vector2.zero;
        crosshairRect.sizeDelta = new Vector2(10f, 10f);

        var crosshairImage = _crosshairRoot.AddComponent<Image>();
        crosshairImage.color = new Color(1f, 1f, 1f, 0.92f);
        crosshairImage.raycastTarget = false;
    }

    private static Font LoadFont()
    {
        var font = Font.CreateDynamicFontFromOSFont(
            new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Arial" },
            16);
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    private static GameObject CreateUiNode(string nodeName, Transform parent)
    {
        var node = new GameObject(nodeName, typeof(RectTransform));
        node.transform.SetParent(parent, false);
        return node;
    }

    private static Text CreateText(
        string nodeName,
        Transform parent,
        Font font,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment)
    {
        var node = CreateUiNode(nodeName, parent);
        var text = node.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void StretchToFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void StretchWithPadding(RectTransform rectTransform, float padding)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(padding, padding);
        rectTransform.offsetMax = new Vector2(-padding, -padding);
    }

    private static bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private static bool WasClosePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var leftClickPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        var escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        return leftClickPressed || escapePressed;
#else
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
