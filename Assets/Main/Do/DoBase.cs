using UnityEngine;
using UnityEngine.EventSystems;

public class DoBase : MonoBehaviour, IXrSelectable
{
    [SerializeField] private Camera fallbackInteractionCamera;
    [SerializeField] private float fallbackRayDistance = 200f;

    public void Select(XrSelectContext context)
    {
        OnSelected(context);
    }

    protected virtual void OnSelected(XrSelectContext context)
    {
    }

    protected virtual void Update()
    {
        if (!XrMouseInput.WasPrimaryPressedThisFrame() || IsPointerOverUI())
        {
            return;
        }

        if (!TryGetMouseFallbackHit(out var hit))
        {
            return;
        }

        var hitTransform = hit.collider.transform;
        if (hitTransform != transform && !hitTransform.IsChildOf(transform))
        {
            return;
        }

        Select(XrSelectContext.MouseFallback(gameObject, hit, true));
    }

    protected bool IsPointerOverUI()
    {
        return XrMouseInput.IsPointerOverUi();
    }

    private bool TryGetMouseFallbackHit(out RaycastHit hit)
    {
        var cameraToUse = ResolveFallbackInteractionCamera();
        if (cameraToUse == null)
        {
            hit = default;
            return false;
        }

        var pointerScreenPosition = XrMouseInput.GetPointerScreenPosition();
        var ray = Cursor.lockState == CursorLockMode.Locked
            ? cameraToUse.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
            : cameraToUse.ScreenPointToRay(pointerScreenPosition);

        return Physics.Raycast(ray, out hit, fallbackRayDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private Camera ResolveFallbackInteractionCamera()
    {
        if (fallbackInteractionCamera != null)
        {
            return fallbackInteractionCamera;
        }

        fallbackInteractionCamera = Camera.main;
        if (fallbackInteractionCamera == null)
        {
            fallbackInteractionCamera = FindFirstObjectByType<Camera>();
        }

        return fallbackInteractionCamera;
    }
}
