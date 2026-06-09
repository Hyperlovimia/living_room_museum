using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class TeleportClickInteractable : MonoBehaviour
{
    [SerializeField] private Transform playerController;
    [SerializeField] private Transform target;
    [SerializeField] private float interactDistance = 12f;
    [SerializeField] private string label = "返回大厅";
    [SerializeField] private bool createWorldLabel = true;

    private Camera _camera;

    public void Configure(Transform player, Transform teleportTarget, string buttonLabel, bool showWorldLabel = true)
    {
        playerController = player;
        target = teleportTarget;
        label = buttonLabel;
        createWorldLabel = showWorldLabel;

        if (createWorldLabel && transform.Find("WorldLabel") == null)
        {
            CreateWorldLabel();
        }
    }

    private void Awake()
    {
        if (createWorldLabel && transform.Find("WorldLabel") == null)
        {
            CreateWorldLabel();
        }
    }

    private void Update()
    {
        if (!WasInteractPressed() || playerController == null || target == null)
        {
            return;
        }

        var cameraToUse = ResolveCamera();
        if (cameraToUse == null)
        {
            return;
        }

        var ray = cameraToUse.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out var hit, interactDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            return;
        }

        if (hit.collider.transform != transform && !hit.collider.transform.IsChildOf(transform))
        {
            return;
        }

        Teleport();
    }

    private void Teleport()
    {
        var movementBounds = playerController.GetComponentInChildren<PlayerMovementBounds>(true);
        if (movementBounds != null)
        {
            movementBounds.enabled = false;
        }

        var firstPersonController = playerController.GetComponentInChildren<FirstPersonController>(true);
        if (firstPersonController != null)
        {
            firstPersonController.enabled = false;
        }

        var characterController = playerController.GetComponentInChildren<CharacterController>(true);
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        playerController.SetPositionAndRotation(target.position, target.rotation);

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (firstPersonController != null)
        {
            firstPersonController.enabled = true;
        }
    }

    private Camera ResolveCamera()
    {
        if (_camera != null)
        {
            return _camera;
        }

        _camera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        return _camera;
    }

    private void CreateWorldLabel()
    {
        var font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Arial" }, 16);
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        var canvasGo = new GameObject("WorldLabel", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        canvasGo.transform.localPosition = new Vector3(0f, 0.18f, -0.035f);
        canvasGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.0068f;

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rect = canvasGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(240f, 88f);

        var background = canvasGo.AddComponent<Image>();
        background.color = new Color(0.08f, 0.055f, 0.035f, 0.82f);
        background.raycastTarget = false;

        var accentGo = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accentGo.transform.SetParent(canvasGo.transform, false);
        var accentRect = accentGo.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(7f, 0f);

        var accent = accentGo.GetComponent<Image>();
        accent.color = new Color(0.85f, 0.66f, 0.33f, 1f);
        accent.raycastTarget = false;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(canvasGo.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 12f);
        textRect.offsetMax = new Vector2(-16f, -10f);

        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.text = label + "\n回到起始展厅";
        text.fontSize = 22;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleLeft;
        text.lineSpacing = 0.86f;
        text.color = new Color(1f, 0.93f, 0.72f, 1f);
        text.raycastTarget = false;
    }

    private static bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }
}
