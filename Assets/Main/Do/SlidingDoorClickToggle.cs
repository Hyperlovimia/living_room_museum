using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class SlidingDoorClickToggle : MonoBehaviour, IXrSelectable
{
    public Transform Target;
    public float ClosedZ = 0f;
    public float OpenZ = -10f;
    public float MoveDuration = 0.6f;
    public float RaycastDistance = 200f;
    public bool StartsOpened;

    private Camera _interactionCamera;
    private bool _isOpen;
    private Tween _moveTween;
    private Vector3 _closedLocalPosition;
    private Vector3 _openedLocalPosition;

    private void Awake()
    {
        Target = Target == null ? transform : Target;

        _closedLocalPosition = Target.localPosition;
        _closedLocalPosition.z = ClosedZ;

        _openedLocalPosition = _closedLocalPosition;
        _openedLocalPosition.z = OpenZ;

        _isOpen = StartsOpened;
        Target.localPosition = _isOpen ? _openedLocalPosition : _closedLocalPosition;
    }

    private void OnDestroy()
    {
        _moveTween?.Kill();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || IsPointerOverUi())
        {
            return;
        }

        if (!TryGetHit(out var hit))
        {
            return;
        }

        var hitTransform = hit.collider.transform;
        if (hitTransform != transform && !hitTransform.IsChildOf(transform))
        {
            return;
        }

        ToggleDoor();
    }

    public void Select(XrSelectContext context)
    {
        ToggleDoor();
    }

    private bool TryGetHit(out RaycastHit hit)
    {
        var cameraToUse = GetInteractionCamera();
        if (cameraToUse == null)
        {
            hit = default;
            return false;
        }

        var ray = Cursor.lockState == CursorLockMode.Locked
            ? cameraToUse.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
            : cameraToUse.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(ray, out hit, RaycastDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private Camera GetInteractionCamera()
    {
        if (_interactionCamera != null)
        {
            return _interactionCamera;
        }

        _interactionCamera = Camera.main;
        if (_interactionCamera == null)
        {
            _interactionCamera = FindFirstObjectByType<Camera>();
        }

        return _interactionCamera;
    }

    private void ToggleDoor()
    {
        _isOpen = !_isOpen;
        _moveTween?.Kill();

        var targetPosition = _isOpen ? _openedLocalPosition : _closedLocalPosition;
        _moveTween = Target.DOLocalMove(targetPosition, MoveDuration);
    }

    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
