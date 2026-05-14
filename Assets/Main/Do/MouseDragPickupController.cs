using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class MouseDragPickupController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float pickupDistance = 8f;
    [SerializeField] private float holdDistance = 3f;
    [SerializeField] private LayerMask draggableLayers = ~0;
    [SerializeField] private LayerMask supportSurfaceLayers = ~0;
    [SerializeField] private float followSpeed = 18f;
    [SerializeField] private float maxThrowChargeDuration = 1.2f;
    [SerializeField] private float minThrowForce = 2.5f;
    [SerializeField] private float maxThrowForce = 10f;
    [SerializeField] private float throwUpwardBias = 0.15f;

    private DraggableObject _heldObject;
    private float _currentHoldDistance;
    private Vector3 _grabOffset;
    private bool _isChargingThrow;
    private float _throwChargeTime;

    private void Awake()
    {
        ResolveCamera();
    }

    private void OnValidate()
    {
        pickupDistance = Mathf.Max(0.1f, pickupDistance);
        holdDistance = Mathf.Max(0.1f, holdDistance);
        followSpeed = Mathf.Max(0.1f, followSpeed);
        maxThrowChargeDuration = Mathf.Max(0.05f, maxThrowChargeDuration);
        minThrowForce = Mathf.Max(0f, minThrowForce);
        maxThrowForce = Mathf.Max(minThrowForce, maxThrowForce);
        throwUpwardBias = Mathf.Max(0f, throwUpwardBias);
    }

    private void Update()
    {
        if (WasPrimaryClickPressed())
        {
            if (_heldObject == null)
            {
                TryPickup();
            }
            else if (!_isChargingThrow)
            {
                ReleaseHeldObject();
            }
        }

        if (_heldObject != null)
        {
            MoveHeldObject();
            HandleThrowInput();
        }
    }

    private void TryPickup()
    {
        if (IsPointerOverUI() || !ResolveCamera())
        {
            return;
        }

        var ray = GetAimRay();
        if (!Physics.Raycast(ray, out var hit, pickupDistance, draggableLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        var draggableObject = hit.collider.GetComponentInParent<DraggableObject>();
        if (draggableObject == null)
        {
            return;
        }

        _heldObject = draggableObject;
        var grabbedDistance = Vector3.Distance(targetCamera.transform.position, hit.point);
        _currentHoldDistance = Mathf.Min(grabbedDistance, holdDistance);
        _grabOffset = draggableObject.transform.position - hit.point;
        _heldObject.BeginDrag();
        ResetThrowCharge();
    }

    private void MoveHeldObject()
    {
        var ray = GetAimRay();
        var targetPosition = ray.GetPoint(_currentHoldDistance) + _grabOffset;
        var smoothedPosition = Vector3.Lerp(
            _heldObject.transform.position,
            targetPosition,
            1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        _heldObject.MoveTo(smoothedPosition);
    }

    private void ReleaseHeldObject()
    {
        var releasedObject = _heldObject;
        _heldObject = null;
        ResetThrowCharge();
        releasedObject.TryLandBelow(supportSurfaceLayers);
    }

    private void HandleThrowInput()
    {
        if (WasSecondaryClickPressed())
        {
            _isChargingThrow = true;
            _throwChargeTime = 0f;
        }

        if (!_isChargingThrow)
        {
            return;
        }

        if (IsSecondaryClickHeld())
        {
            _throwChargeTime = Mathf.Min(_throwChargeTime + Time.deltaTime, maxThrowChargeDuration);
        }

        if (WasSecondaryClickReleased())
        {
            ThrowHeldObject();
        }
    }

    private void ThrowHeldObject()
    {
        var thrownObject = _heldObject;
        _heldObject = null;

        var chargeRatio = Mathf.Clamp01(_throwChargeTime / maxThrowChargeDuration);
        var throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRatio);
        var throwDirection = (targetCamera.transform.forward + Vector3.up * throwUpwardBias).normalized;

        ResetThrowCharge();
        thrownObject.Throw(throwDirection, throwForce, supportSurfaceLayers);
    }

    private void ResetThrowCharge()
    {
        _isChargingThrow = false;
        _throwChargeTime = 0f;
    }

    private bool ResolveCamera()
    {
        if (targetCamera != null)
        {
            return true;
        }

        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindAnyObjectByType<Camera>();
        }

        return targetCamera != null;
    }

    private static bool WasPrimaryClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private static bool WasSecondaryClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(1);
#endif
    }

    private static bool IsSecondaryClickHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        return Input.GetMouseButton(1);
#endif
    }

    private static bool WasSecondaryClickReleased()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(1);
#endif
    }

    private Ray GetAimRay()
    {
        return targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }

    private static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return EventSystem.current.IsPointerOverGameObject(Mouse.current.deviceId);
        }
#endif
        return EventSystem.current.IsPointerOverGameObject();
    }
}
