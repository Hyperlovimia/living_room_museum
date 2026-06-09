using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class SpawnRoomController : MonoBehaviour
{
    [Header("传送")]
    public Transform playerController;
    public Transform studyTeleportTarget;
    public Transform spawnTeleportTarget;

    [Header("交互按钮")]
    public SpawnInteractable teleportButton;
    public SpawnInteractable findGameButton;

    [Header("开场白")]
    public AudioSource narrationSource;
    public AudioClip narrationClip;
    public bool playNarrationOnStart = true;

    [Header("找一找游戏")]
    public ExhibitInfo findTargetInfo;
    public Collider findTargetCollider;
    public float interactDistance = 12f;

    [Header("墙面文字 (世界空间)")]
    public Transform wallTextAnchor;
    [TextArea(4, 12)]
    public string wallTitle = "中华起居文化";
    [TextArea(4, 12)]
    public string wallBody = "起居，谓人之日常作息，亦泛指室内陈设与礼制。中华起居文化源远流长，自席地而坐，至椅榻并陈，几案屏帐间，皆见礼乐之节、雅俗之分。\n\n本馆以书房一隅为引，邀君步入古人之居室，于器物之中，照见千年文心。";
    public float wallTextWidth = 8f;
    public float wallTextHeight = 3.2f;
    public bool revealWallTextOnStart = true;
    public float fallbackWallRevealDuration = 1f;
    public float wallTypewriterCharactersPerSecond = 26f;

    [Header("按钮文字")]
    public Transform teleportButtonTextAnchor;
    public string teleportButtonText = "进入书房";
    public string teleportButtonDescription = "参观起居文化展厅";
    public Transform findButtonTextAnchor;
    public string findButtonText = "找一找";
    public string findButtonDescription = "开始寻找指定展品";

    private Camera _mainCamera;
    private FirstPersonController _firstPersonController;
    private StarterAssetsInputs _starterAssetsInputs;
    private ExhibitInteractionController _exhibitInteractionController;
    private PlayerMovementBounds _playerMovementBounds;

    private Canvas _uiCanvas;
    private GameObject _overlayRoot;
    private GameObject _crosshairRoot;
    private Text _titleText;
    private Text _descriptionText;
    private Text _hintText;
    private GameObject _findHudRoot;
    private Text _findHudText;
    private GameObject _toastRoot;
    private Text _toastText;
    private Text _wallTitleText;
    private Text _wallBodyText;
    private string _wallTitleFullText;
    private string _wallBodyFullText;
    private float _wallRevealStartedAt;

    private bool _isPanelOpen;
    private bool _isFindMode;
    private bool _isWallTextRevealing;

    private void Awake()
    {
        _mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        if (playerController != null)
        {
            _firstPersonController = playerController.GetComponentInChildren<FirstPersonController>(true);
            _starterAssetsInputs = playerController.GetComponentInChildren<StarterAssetsInputs>(true);
            _exhibitInteractionController = playerController.GetComponentInChildren<ExhibitInteractionController>(true);
            _playerMovementBounds = playerController.GetComponentInChildren<PlayerMovementBounds>(true);
            if (_playerMovementBounds != null) _playerMovementBounds.enabled = false;
        }

        if (narrationSource == null)
        {
            narrationSource = GetComponent<AudioSource>();
            if (narrationSource == null)
            {
                narrationSource = gameObject.AddComponent<AudioSource>();
            }
        }
        narrationSource.playOnAwake = false;
        narrationSource.loop = false;
        narrationSource.spatialBlend = 0f;

        BuildUi();
        BuildWorldTexts();
        StyleSpawnButtons();
        SetPanelVisible(false);
        SetFindHudVisible(false);
        SetToastVisible(false);
    }

    private void Start()
    {
        if (findTargetCollider == null && findTargetInfo != null)
        {
            findTargetCollider = findTargetInfo.GetComponentInChildren<Collider>();
        }
        if (playNarrationOnStart && narrationClip != null && narrationSource != null)
        {
            narrationSource.clip = narrationClip;
            narrationSource.Play();
        }

        if (revealWallTextOnStart)
        {
            BeginWallTextReveal();
        }
    }

    private void Update()
    {
        UpdateWallTextReveal();

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
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (_mainCamera == null) return;
        }

        var ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out var hit, interactDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            return;
        }

        var interactable = hit.collider.GetComponentInParent<SpawnInteractable>();
        if (interactable != null)
        {
            HandleInteractable(interactable);
            return;
        }

        if (_isFindMode && findTargetCollider != null && hit.collider == findTargetCollider)
        {
            OnFindTargetClicked();
        }
    }

    private void HandleInteractable(SpawnInteractable interactable)
    {
        if (interactable == teleportButton)
        {
            TeleportToStudy();
        }
        else if (interactable == findGameButton)
        {
            OpenFindGameIntro();
        }
    }

    private void TeleportToStudy()
    {
        if (playerController == null || studyTeleportTarget == null) return;
        MovePlayerTo(studyTeleportTarget.position, studyTeleportTarget.rotation);
        if (_playerMovementBounds != null) _playerMovementBounds.enabled = true;
        if (_isFindMode)
        {
            SetFindHudVisible(false);
            _isFindMode = false;
            if (_exhibitInteractionController != null) _exhibitInteractionController.enabled = true;
        }
    }

    private void OpenFindGameIntro()
    {
        if (findTargetInfo == null) return;
        _findIntroJustOpened = true;
        OpenPanel(findTargetInfo.ExhibitTitle, findTargetInfo.Description, "左键或 Esc 关闭，开始寻找");
    }

    private void OpenPanel(string title, string description, string hint)
    {
        _isPanelOpen = true;
        _titleText.text = title;
        _descriptionText.text = description;
        _hintText.text = hint;

        if (_firstPersonController != null) _firstPersonController.enabled = false;
        if (_starterAssetsInputs != null)
        {
            _starterAssetsInputs.cursorLocked = true;
            _starterAssetsInputs.cursorInputForLook = false;
            _starterAssetsInputs.MoveInput(Vector2.zero);
            _starterAssetsInputs.LookInput(Vector2.zero);
        }

        SetPanelVisible(true);
    }

    private void ClosePanel()
    {
        _isPanelOpen = false;
        var enterFind = _findIntroJustOpened && !_isFindMode;
        _findIntroJustOpened = false;

        if (_firstPersonController != null) _firstPersonController.enabled = true;
        if (_starterAssetsInputs != null)
        {
            _starterAssetsInputs.cursorLocked = true;
            _starterAssetsInputs.cursorInputForLook = true;
            _starterAssetsInputs.MoveInput(Vector2.zero);
            _starterAssetsInputs.LookInput(Vector2.zero);
        }
        SetPanelVisible(false);

        if (enterFind && findTargetInfo != null)
        {
            EnterFindMode();
        }
    }

    private bool _findIntroJustOpened;

    private void EnterFindMode()
    {
        _isFindMode = true;
        if (_findHudText != null) _findHudText.text = $"正在寻找：{findTargetInfo.ExhibitTitle}";
        SetFindHudVisible(true);
        if (_exhibitInteractionController != null) _exhibitInteractionController.enabled = false;
    }

    private void OnFindTargetClicked()
    {
        _isFindMode = false;
        SetFindHudVisible(false);
        if (_exhibitInteractionController != null) _exhibitInteractionController.enabled = true;
        ShowToast($"恭喜！你找到了{findTargetInfo.ExhibitTitle}");
        Invoke(nameof(TeleportBackToSpawn), 1.6f);
    }

    private void TeleportBackToSpawn()
    {
        SetToastVisible(false);
        if (_playerMovementBounds != null) _playerMovementBounds.enabled = false;
        if (playerController != null && spawnTeleportTarget != null)
        {
            MovePlayerTo(spawnTeleportTarget.position, spawnTeleportTarget.rotation);
        }
    }

    private void MovePlayerTo(Vector3 position, Quaternion rotation)
    {
        var characterController = playerController.GetComponentInChildren<CharacterController>(true);
        var targetTransform = characterController != null ? characterController.transform : playerController;

        if (_firstPersonController != null) _firstPersonController.enabled = false;
        if (characterController != null) characterController.enabled = false;
        ClearMovementInput();

        targetTransform.SetPositionAndRotation(position, rotation);

        if (characterController != null) characterController.enabled = true;
        if (_firstPersonController != null) _firstPersonController.enabled = true;
        ClearMovementInput();
    }

    private void ClearMovementInput()
    {
        if (_starterAssetsInputs == null)
        {
            return;
        }

        _starterAssetsInputs.MoveInput(Vector2.zero);
        _starterAssetsInputs.LookInput(Vector2.zero);
    }

    private void ShowToast(string message)
    {
        if (_toastText != null) _toastText.text = message;
        SetToastVisible(true);
    }

    private void SetPanelVisible(bool visible)
    {
        if (_overlayRoot != null) _overlayRoot.SetActive(visible);
        if (_crosshairRoot != null) _crosshairRoot.SetActive(!visible);
    }

    private void SetFindHudVisible(bool visible)
    {
        if (_findHudRoot != null) _findHudRoot.SetActive(visible);
    }

    private void SetToastVisible(bool visible)
    {
        if (_toastRoot != null) _toastRoot.SetActive(visible);
    }

    private void BuildWorldTexts()
    {
        var font = LoadFont();
        if (wallTextAnchor != null)
        {
            var wallTexts = BuildWorldPanel(wallTextAnchor, wallTextWidth, wallTextHeight,
                wallTitle, wallBody, font, 0.012f, true);
            _wallTitleText = wallTexts.Title;
            _wallBodyText = wallTexts.Body;
            _wallTitleFullText = wallTitle;
            _wallBodyFullText = wallBody;
        }
        if (teleportButtonTextAnchor != null)
        {
            BuildButtonSign(teleportButtonTextAnchor, teleportButtonText, teleportButtonDescription, font,
                new Color(0.87f, 0.58f, 0.22f, 1f));
        }
        if (findButtonTextAnchor != null)
        {
            BuildButtonSign(findButtonTextAnchor, findButtonText, findButtonDescription, font,
                new Color(0.36f, 0.63f, 0.52f, 1f));
        }
    }

    private void StyleSpawnButtons()
    {
        StyleSpawnButton(teleportButton);
        StyleSpawnButton(findGameButton);
    }

    private static void StyleSpawnButton(SpawnInteractable button)
    {
        if (button == null)
        {
            return;
        }

        var renderer = button.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    private static void BuildButtonSign(Transform anchor, string buttonTitle, string description, Font font, Color accentColor)
    {
        var canvasGo = new GameObject("ButtonSignCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(anchor, false);
        canvasGo.transform.localPosition = Vector3.zero;
        canvasGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.012f;

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rect = canvasGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(260f, 132f);

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvasGo.transform, false);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.08f, 0.075f, 0.065f, 0.92f);

        var stripe = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        stripe.transform.SetParent(canvasGo.transform, false);
        var stripeRect = stripe.GetComponent<RectTransform>();
        stripeRect.anchorMin = new Vector2(0f, 0f);
        stripeRect.anchorMax = new Vector2(0f, 1f);
        stripeRect.pivot = new Vector2(0f, 0.5f);
        stripeRect.anchoredPosition = Vector2.zero;
        stripeRect.sizeDelta = new Vector2(10f, 0f);
        stripe.GetComponent<Image>().color = accentColor;

        var title = CreateText("Title", canvasGo.transform, font, 28, FontStyle.Bold, TextAnchor.UpperLeft);
        title.text = buttonTitle;
        title.color = new Color(1f, 0.93f, 0.74f, 1f);
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(28f, -18f);
        titleRect.sizeDelta = new Vector2(-48f, 34f);

        var body = CreateText("Description", canvasGo.transform, font, 16, FontStyle.Normal, TextAnchor.UpperLeft);
        body.text = description;
        body.color = new Color(0.92f, 0.92f, 0.88f, 1f);
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        var bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(28f, 38f);
        bodyRect.offsetMax = new Vector2(-24f, -58f);

        var hint = CreateText("Hint", canvasGo.transform, font, 14, FontStyle.Italic, TextAnchor.LowerRight);
        hint.text = "左键点击";
        hint.color = accentColor;
        var hintRect = hint.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(1f, 0f);
        hintRect.anchoredPosition = new Vector2(-18f, 12f);
        hintRect.sizeDelta = new Vector2(-36f, 24f);
    }

    private static WorldPanelTexts BuildWorldPanel(Transform anchor, float width, float height,
        string title, string body, Font font, float pixelScale, bool isWallIntro)
    {
        var canvasGo = new GameObject("WorldCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(anchor, false);
        canvasGo.transform.localPosition = Vector3.zero;
        canvasGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        canvasGo.transform.localScale = Vector3.one * pixelScale;

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rect = canvasGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width / pixelScale, height / pixelScale);

        Text titleText = null;
        var pad = isWallIntro ? 86f : 40f;
        if (!string.IsNullOrEmpty(title))
        {
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(canvasGo.transform, false);
            var t = titleGo.AddComponent<Text>();
            t.font = font;
            t.text = title;
            t.fontSize = isWallIntro ? 40 : 72;
            t.fontStyle = FontStyle.Bold;
            t.color = new Color(0.95f, 0.88f, 0.7f);
            t.alignment = TextAnchor.UpperLeft;
            t.raycastTarget = false;
            var tr = t.rectTransform;
            tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
            tr.pivot = new Vector2(0f, 1f);
            tr.sizeDelta = new Vector2(-pad * 2, isWallIntro ? 56 : 96);
            tr.anchoredPosition = new Vector2(pad, isWallIntro ? -18f : -pad);
            titleText = t;
        }

        var bodyGo = new GameObject("Body", typeof(RectTransform));
        bodyGo.transform.SetParent(canvasGo.transform, false);
        var b = bodyGo.AddComponent<Text>();
        b.font = font;
        b.text = body;
        b.fontSize = string.IsNullOrEmpty(title) ? 42 : 20;
        b.fontStyle = string.IsNullOrEmpty(title) ? FontStyle.Bold : FontStyle.Normal;
        b.color = Color.white;
        b.alignment = string.IsNullOrEmpty(title) ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft;
        b.horizontalOverflow = HorizontalWrapMode.Wrap;
        b.verticalOverflow = VerticalWrapMode.Overflow;
        b.lineSpacing = isWallIntro ? 1.16f : 1.08f;
        b.raycastTarget = false;
        var br = b.rectTransform;
        br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one;
        br.offsetMin = new Vector2(pad, isWallIntro ? 62f : pad);
        br.offsetMax = new Vector2(-pad, string.IsNullOrEmpty(title) ? -pad : -(isWallIntro ? 86f : 156f));
        return new WorldPanelTexts(titleText, b);
    }

    private void BeginWallTextReveal()
    {
        if (_wallTitleText == null || _wallBodyText == null)
        {
            return;
        }

        _wallRevealStartedAt = Time.time;
        _isWallTextRevealing = true;
        _wallTitleText.text = string.Empty;
        _wallBodyText.text = string.Empty;
    }

    private void UpdateWallTextReveal()
    {
        if (!_isWallTextRevealing)
        {
            return;
        }

        var elapsed = Time.time - _wallRevealStartedAt;
        var charactersToShow = Mathf.FloorToInt(elapsed * Mathf.Max(1f, wallTypewriterCharactersPerSecond));
        var titleLength = string.IsNullOrEmpty(_wallTitleFullText) ? 0 : _wallTitleFullText.Length;
        var bodyCharacters = charactersToShow - titleLength;

        _wallTitleText.text = TakeChars(_wallTitleFullText, charactersToShow);
        _wallBodyText.text = bodyCharacters > 0 ? TakeChars(_wallBodyFullText, bodyCharacters) : string.Empty;

        var bodyLength = string.IsNullOrEmpty(_wallBodyFullText) ? 0 : _wallBodyFullText.Length;
        if (charactersToShow >= titleLength + bodyLength)
        {
            _wallTitleText.text = _wallTitleFullText;
            _wallBodyText.text = _wallBodyFullText;
            _isWallTextRevealing = false;
        }
    }

    private static string TakeChars(string source, int length)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        var clampedLength = Mathf.Clamp(length, 0, source.Length);
        return source.Substring(0, clampedLength);
    }

    private readonly struct WorldPanelTexts
    {
        public readonly Text Title;
        public readonly Text Body;

        public WorldPanelTexts(Text title, Text body)
        {
            Title = title;
            Body = body;
        }
    }

    private void BuildUi()
    {
        var font = LoadFont();

        var canvasGo = new GameObject("SpawnRoomCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        _uiCanvas = canvasGo.GetComponent<Canvas>();
        _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _uiCanvas.sortingOrder = 900;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        _overlayRoot = CreateUiNode("Overlay", canvasGo.transform);
        StretchToFullScreen(_overlayRoot.GetComponent<RectTransform>());
        var overlayImg = _overlayRoot.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.5f);

        var panel = CreateUiNode("Panel", _overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 460f);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.09f, 0.08f, 0.94f);

        _titleText = CreateText("Title", panel.transform, font, 32, FontStyle.Bold, TextAnchor.UpperLeft);
        var tr = _titleText.rectTransform;
        tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0f, 1f);
        tr.anchoredPosition = new Vector2(28f, -24f); tr.sizeDelta = new Vector2(-56f, 42f);

        _descriptionText = CreateText("Description", panel.transform, font, 22, FontStyle.Normal, TextAnchor.UpperLeft);
        var dr = _descriptionText.rectTransform;
        dr.anchorMin = new Vector2(0f, 0f); dr.anchorMax = new Vector2(1f, 1f); dr.pivot = new Vector2(0f, 1f);
        dr.offsetMin = new Vector2(28f, 64f); dr.offsetMax = new Vector2(-28f, -82f);
        _descriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _descriptionText.verticalOverflow = VerticalWrapMode.Overflow;
        _descriptionText.lineSpacing = 1.2f;

        _hintText = CreateText("Hint", panel.transform, font, 18, FontStyle.Italic, TextAnchor.LowerRight);
        var hr = _hintText.rectTransform;
        hr.anchorMin = new Vector2(0f, 0f); hr.anchorMax = new Vector2(1f, 0f); hr.pivot = new Vector2(1f, 0f);
        hr.anchoredPosition = new Vector2(-28f, 18f); hr.sizeDelta = new Vector2(-56f, 28f);
        _hintText.color = new Color(1f, 1f, 1f, 0.8f);

        _crosshairRoot = CreateUiNode("Crosshair", canvasGo.transform);
        var cr = _crosshairRoot.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0.5f);
        cr.sizeDelta = new Vector2(10f, 10f);
        var cImg = _crosshairRoot.AddComponent<Image>();
        cImg.color = new Color(1f, 1f, 1f, 0.92f);
        cImg.raycastTarget = false;

        _findHudRoot = CreateUiNode("FindHud", canvasGo.transform);
        var fhr = _findHudRoot.GetComponent<RectTransform>();
        fhr.anchorMin = new Vector2(0f, 1f); fhr.anchorMax = new Vector2(0f, 1f); fhr.pivot = new Vector2(0f, 1f);
        fhr.anchoredPosition = new Vector2(24f, -24f); fhr.sizeDelta = new Vector2(520f, 48f);
        var fhBg = _findHudRoot.AddComponent<Image>();
        fhBg.color = new Color(0f, 0f, 0f, 0.55f);
        _findHudText = CreateText("Text", _findHudRoot.transform, font, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        var fht = _findHudText.rectTransform;
        StretchWithPadding(fht, 16f);

        _toastRoot = CreateUiNode("Toast", canvasGo.transform);
        var tor = _toastRoot.GetComponent<RectTransform>();
        tor.anchorMin = tor.anchorMax = tor.pivot = new Vector2(0.5f, 0.5f);
        tor.sizeDelta = new Vector2(640f, 120f);
        var toastBg = _toastRoot.AddComponent<Image>();
        toastBg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);
        _toastText = CreateText("Text", _toastRoot.transform, font, 36, FontStyle.Bold, TextAnchor.MiddleCenter);
        StretchWithPadding(_toastText.rectTransform, 24f);
    }

    private static Font LoadFont()
    {
        var font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Arial" }, 16);
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    private static GameObject CreateUiNode(string nodeName, Transform parent)
    {
        var node = new GameObject(nodeName, typeof(RectTransform));
        node.transform.SetParent(parent, false);
        return node;
    }

    private static Text CreateText(string nodeName, Transform parent, Font font, int fontSize, FontStyle style, TextAnchor align)
    {
        var node = CreateUiNode(nodeName, parent);
        var text = node.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = align;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void StretchToFullScreen(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void StretchWithPadding(RectTransform rt, float pad)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, pad); rt.offsetMax = new Vector2(-pad, -pad);
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
        var l = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        var e = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        return l || e;
#else
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
