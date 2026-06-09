using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

[DisallowMultipleComponent]
public class XrWorldPanelPresenter : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float panelDistance = 1.7f;
    [SerializeField] private float metersPerPixel = 0.0011f;
    [SerializeField] private Vector3 cameraSpaceOffset = new Vector3(0f, -0.05f, 0f);

    private static XrWorldPanelPresenter _instance;

    public static XrWorldPanelPresenter Instance => GetOrCreate();

    public static XrWorldPanelPresenter GetOrCreate()
    {
        if (_instance != null)
        {
            return _instance;
        }

        _instance = FindFirstObjectByType<XrWorldPanelPresenter>(FindObjectsInactive.Include);
        if (_instance != null)
        {
            return _instance;
        }

        var owner = new GameObject("XR World Panel Presenter");
        _instance = owner.AddComponent<XrWorldPanelPresenter>();
        return _instance;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public Canvas CreateCameraPanelCanvas(
        string canvasName,
        Transform owner,
        int sortingOrder,
        Vector2 referenceResolution)
    {
        var canvasRoot = new GameObject(canvasName, typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
        canvasRoot.transform.SetParent(owner, false);

        var canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = sortingOrder;

        var scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var rect = canvasRoot.GetComponent<RectTransform>();
        rect.sizeDelta = referenceResolution;
        canvasRoot.transform.localScale = Vector3.one * metersPerPixel;
        PlaceInFront(canvas);
        return canvas;
    }

    public void PlaceInFront(Canvas canvas)
    {
        if (canvas == null)
        {
            return;
        }

        PlaceInFront(canvas.transform);
    }

    public void PlaceInFront(Transform panelTransform)
    {
        var cameraToUse = ResolveCamera();
        if (panelTransform == null || cameraToUse == null)
        {
            return;
        }

        var cameraTransform = cameraToUse.transform;
        panelTransform.position = cameraTransform.position
                                  + cameraTransform.forward * panelDistance
                                  + cameraTransform.TransformVector(cameraSpaceOffset);
        panelTransform.rotation = Quaternion.LookRotation(panelTransform.position - cameraTransform.position, Vector3.up);
    }

    public static void EnsureTrackedRaycaster(Canvas canvas)
    {
        if (canvas == null || canvas.GetComponent<TrackedDeviceGraphicRaycaster>() != null)
        {
            return;
        }

        canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
    }

    public static Button AddCloseButton(Transform parent, Font font, UnityAction onClick)
    {
        var buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-18f, -18f);
        rect.sizeDelta = new Vector2(44f, 44f);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.95f, 0.82f, 0.54f, 0.95f);

        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var label = labelObject.GetComponent<Text>();
        label.font = font;
        label.text = "X";
        label.fontSize = 24;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.12f, 0.09f, 0.05f, 1f);
        label.raycastTarget = false;

        return button;
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null)
        {
            return targetCamera;
        }

        var playerAdapter = XROriginPlayerAdapter.Resolve();
        if (playerAdapter != null)
        {
            targetCamera = playerAdapter.Camera;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        }

        return targetCamera;
    }
}
