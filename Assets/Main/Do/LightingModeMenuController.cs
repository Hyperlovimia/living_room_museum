using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class LightingModeMenuController : DoBase
{
    [SerializeField] private string panelTitle = "灯光模式";
    [SerializeField] private bool autoCreateCollider = true;
    [SerializeField] private Vector2 panelSize = new Vector2(360f, 300f);
    [SerializeField] private Vector2 buttonSize = new Vector2(220f, 38f);
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color panelColor = new Color(0.1f, 0.09f, 0.08f, 0.94f);
    [Header("Background Music")]
    [SerializeField] private AudioClip morningReadingMusic;
    [SerializeField] private AudioClip guestModeMusic;
    [SerializeField] private AudioClip teaModeMusic;
    [Header("Skybox Materials")]
    [SerializeField] private Material morningReadingSkybox;
    [SerializeField] private Material guestModeSkybox;
    [SerializeField] private Material teaModeSkybox;

    private readonly System.Collections.Generic.List<ActiveDo> activeLights = new System.Collections.Generic.List<ActiveDo>();

    private Camera mainCamera;
    private FirstPersonController firstPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private XROriginPlayerAdapter xrPlayer;
    private Canvas uiCanvas;
    private GameObject overlayRoot;
    private Button morningReadingButton;
    private Button guestModeButton;
    private Button teaModeButton;
    private Font menuFont;
    private ActiveDo.LightingMode? currentMode;
    private bool isPanelOpen;

    private void Awake()
    {
        if (autoCreateCollider && GetComponent<Collider>() == null)
        {
            FitColliderToChildren();
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
        }

        firstPersonController = FindAnyObjectByType<FirstPersonController>();
        starterAssetsInputs = FindAnyObjectByType<StarterAssetsInputs>();
        xrPlayer = XROriginPlayerAdapter.Resolve();

        BuildUi();
        CacheActiveLights();
        SetPanelVisible(false);
    }

    private void Update()
    {
        if (isPanelOpen && WasClosePressed())
        {
            ClosePanel();
        }
    }

    protected override void OnSelected(XrSelectContext context)
    {
        if (isPanelOpen)
        {
            return;
        }

        OpenPanel();
    }

    private void OpenPanel()
    {
        CacheActiveLights();
        isPanelOpen = true;

        if (xrPlayer == null)
        {
            xrPlayer = XROriginPlayerAdapter.Resolve();
        }

        if (xrPlayer != null)
        {
            xrPlayer.SetMovementEnabled(false);
        }

        if (firstPersonController != null)
        {
            firstPersonController.enabled = false;
        }

        if (starterAssetsInputs != null)
        {
            starterAssetsInputs.cursorLocked = false;
            starterAssetsInputs.cursorInputForLook = false;
            starterAssetsInputs.MoveInput(Vector2.zero);
            starterAssetsInputs.LookInput(Vector2.zero);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetPanelVisible(true);
        UpdateButtonVisuals();
    }

    private void ClosePanel()
    {
        isPanelOpen = false;

        if (xrPlayer == null)
        {
            xrPlayer = XROriginPlayerAdapter.Resolve();
        }

        if (xrPlayer != null)
        {
            xrPlayer.SetMovementEnabled(true);
        }

        if (firstPersonController != null)
        {
            firstPersonController.enabled = true;
        }

        if (starterAssetsInputs != null)
        {
            starterAssetsInputs.cursorLocked = true;
            starterAssetsInputs.cursorInputForLook = true;
            starterAssetsInputs.MoveInput(Vector2.zero);
            starterAssetsInputs.LookInput(Vector2.zero);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetPanelVisible(false);
    }

    private void ApplyMode(ActiveDo.LightingMode lightingMode)
    {
        currentMode = lightingMode;

        for (int i = activeLights.Count - 1; i >= 0; i--)
        {
            ActiveDo activeDo = activeLights[i];
            if (activeDo == null)
            {
                activeLights.RemoveAt(i);
                continue;
            }

            activeDo.ApplyLightingMode(lightingMode);
        }

        ApplyBackgroundMusic(lightingMode);
        ApplySkybox(lightingMode);
        UpdateButtonVisuals();
    }

    private void ApplyBackgroundMusic(ActiveDo.LightingMode lightingMode)
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            return;
        }

        AudioClip musicClip = GetMusicClipForMode(lightingMode);
        if (musicClip == null)
        {
            return;
        }

        audioManager.SetBackgroundMusic(musicClip);
    }

    private AudioClip GetMusicClipForMode(ActiveDo.LightingMode lightingMode)
    {
        switch (lightingMode)
        {
            case ActiveDo.LightingMode.MorningReading:
                return morningReadingMusic;
            case ActiveDo.LightingMode.GuestMode:
                return guestModeMusic;
            default:
                return teaModeMusic;
        }
    }

    private void ApplySkybox(ActiveDo.LightingMode lightingMode)
    {
        Material skyboxMaterial = GetSkyboxMaterialForMode(lightingMode);
        if (skyboxMaterial == null || RenderSettings.skybox == skyboxMaterial)
        {
            return;
        }

        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();
    }

    private Material GetSkyboxMaterialForMode(ActiveDo.LightingMode lightingMode)
    {
        switch (lightingMode)
        {
            case ActiveDo.LightingMode.MorningReading:
                return morningReadingSkybox;
            case ActiveDo.LightingMode.GuestMode:
                return guestModeSkybox;
            default:
                return teaModeSkybox;
        }
    }

    private void CacheActiveLights()
    {
        activeLights.Clear();

        ActiveDo[] sceneLights = FindObjectsByType<ActiveDo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (ActiveDo activeDo in sceneLights)
        {
            if (activeDo != null && activeDo.SupportsLightingModes())
            {
                activeLights.Add(activeDo);
            }
        }
    }

    private void SetPanelVisible(bool visible)
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(visible);
        }

        if (visible && uiCanvas != null)
        {
            XrWorldPanelPresenter.GetOrCreate().PlaceInFront(uiCanvas);
        }
    }

    private void BuildUi()
    {
        EnsureEventSystem();

        menuFont = LoadFont();

        uiCanvas = XrWorldPanelPresenter.GetOrCreate()
            .CreateCameraPanelCanvas("LightingModeCanvas", transform, 900, new Vector2(1920f, 1080f));
        GameObject canvasRoot = uiCanvas.gameObject;

        overlayRoot = CreateUiNode("Overlay", canvasRoot.transform);
        RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
        StretchToFullScreen(overlayRect);

        Image overlayImage = overlayRoot.AddComponent<Image>();
        overlayImage.color = overlayColor;

        GameObject panel = CreateUiNode("Panel", overlayRoot.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = panelSize;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = panelColor;
        XrWorldPanelPresenter.AddCloseButton(panel.transform, menuFont, ClosePanel);

        VerticalLayoutGroup layoutGroup = panel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(24, 24, 24, 24);
        layoutGroup.spacing = 12f;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;

        CreateLabel(panel.transform, "Title", panelTitle, 28, new Color(0.96f, 0.94f, 0.86f, 1f), FontStyle.Bold, 260f, 36f);
        morningReadingButton = CreateButton(panel.transform, "MorningReadingButton", "晨读模式", () => ApplyMode(ActiveDo.LightingMode.MorningReading));
        guestModeButton = CreateButton(panel.transform, "GuestModeButton", "会客模式", () => ApplyMode(ActiveDo.LightingMode.GuestMode));
        teaModeButton = CreateButton(panel.transform, "TeaModeButton", "品茶模式", () => ApplyMode(ActiveDo.LightingMode.TeaMode));
        CreateLabel(panel.transform, "Hint", "选择 X 或 Esc 关闭菜单", 18, new Color(1f, 1f, 1f, 0.7f), FontStyle.Italic, 260f, 26f);
    }

    private void EnsureEventSystem()
    {
        GameObject eventSystemObject;
        if (EventSystem.current != null)
        {
            eventSystemObject = EventSystem.current.gameObject;
        }
        else
        {
            eventSystemObject = new GameObject("LightingModeEventSystem");
            eventSystemObject.transform.SetParent(transform, false);
            eventSystemObject.AddComponent<EventSystem>();
        }

        var standaloneInputModule = eventSystemObject.GetComponent<StandaloneInputModule>();
        if (standaloneInputModule != null)
        {
            standaloneInputModule.enabled = false;
        }

        if (eventSystemObject.GetComponent<XRUIInputModule>() == null)
        {
            eventSystemObject.AddComponent<XRUIInputModule>();
        }
    }

    private Button CreateButton(Transform parent, string objectName, string buttonText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateUiNode(objectName, parent);
        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = buttonSize.x;
        layoutElement.preferredHeight = buttonSize.y;

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.21f, 0.25f, 0.31f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        CreateLabel(buttonObject.transform, "Label", buttonText, 22, new Color(0.97f, 0.97f, 0.97f, 1f), FontStyle.Normal, buttonSize.x, buttonSize.y);
        return button;
    }

    private Text CreateLabel(Transform parent, string objectName, string labelText, int fontSize, Color textColor, FontStyle fontStyle, float width, float height)
    {
        GameObject textObject = CreateUiNode(objectName, parent);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(width, height);

        Text text = textObject.AddComponent<Text>();
        text.text = labelText;
        text.font = menuFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = textColor;
        text.raycastTarget = false;
        return text;
    }

    private void UpdateButtonVisuals()
    {
        UpdateButtonVisual(morningReadingButton, currentMode == ActiveDo.LightingMode.MorningReading);
        UpdateButtonVisual(guestModeButton, currentMode == ActiveDo.LightingMode.GuestMode);
        UpdateButtonVisual(teaModeButton, currentMode == ActiveDo.LightingMode.TeaMode);
    }

    private static void UpdateButtonVisual(Button button, bool isSelected)
    {
        if (button == null)
        {
            return;
        }

        Image buttonImage = button.GetComponent<Image>();
        buttonImage.color = isSelected
            ? new Color(0.87f, 0.64f, 0.26f, 0.98f)
            : new Color(0.21f, 0.25f, 0.31f, 0.95f);

        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.color = isSelected
                ? new Color(0.15f, 0.11f, 0.06f, 1f)
                : new Color(0.97f, 0.97f, 0.97f, 1f);
        }
    }

    private static Font LoadFont()
    {
        Font font = Font.CreateDynamicFontFromOSFont(
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
        GameObject node = new GameObject(nodeName, typeof(RectTransform));
        node.transform.SetParent(parent, false);
        return node;
    }

    private static void StretchToFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    [ContextMenu("Fit Box Collider To Children")]
    private void FitColliderToChildren()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            Bounds bounds = renderer.bounds;
            foreach (Vector3 corner in GetBoundsCorners(bounds))
            {
                Vector3 localPoint = transform.InverseTransformPoint(corner);
                if (!hasBounds)
                {
                    localBounds = new Bounds(localPoint, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(localPoint);
                }
            }
        }

        if (!hasBounds)
        {
            return;
        }

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.center = localBounds.center;
        boxCollider.size = localBounds.size;
    }

    private static Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        return new[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z),
        };
    }

    private static bool WasClosePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
