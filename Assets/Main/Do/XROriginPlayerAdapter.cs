using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[DisallowMultipleComponent]
public class XROriginPlayerAdapter : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Camera xrCamera;
    [SerializeField] private Behaviour dynamicMoveProvider;
    [SerializeField] private Behaviour teleportationProvider;
    [SerializeField] private Behaviour turnProvider;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject reticleRoot;

    private readonly List<Behaviour> _movementBehaviours = new List<Behaviour>();

    public static XROriginPlayerAdapter Active { get; private set; }
    public Camera Camera => ResolveCamera();
    public Transform Origin => ResolveOriginTransform();

    private void Awake()
    {
        Active = this;
        ResolveReferences();
    }

    private void OnEnable()
    {
        Active = this;
    }

    public static XROriginPlayerAdapter Resolve()
    {
        if (Active != null)
        {
            return Active;
        }

        Active = FindFirstObjectByType<XROriginPlayerAdapter>(FindObjectsInactive.Include);
        return Active;
    }

    public void MoveTo(Transform target)
    {
        if (target == null)
        {
            return;
        }

        MoveTo(target.position, target.rotation);
    }

    public void MoveTo(Vector3 position, Quaternion rotation)
    {
        ResolveReferences();

        var originTransform = ResolveOriginTransform();
        var cameraToUse = ResolveCamera();
        if (originTransform == null)
        {
            return;
        }

        var wasControllerEnabled = characterController != null && characterController.enabled;
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (cameraToUse != null)
        {
            var cameraTransform = cameraToUse.transform;
            var yawDelta = Mathf.DeltaAngle(cameraTransform.eulerAngles.y, rotation.eulerAngles.y);
            originTransform.RotateAround(cameraTransform.position, Vector3.up, yawDelta);

            var cameraOffset = cameraTransform.position - originTransform.position;
            var nextOriginPosition = position - new Vector3(cameraOffset.x, 0f, cameraOffset.z);
            nextOriginPosition.y = position.y;
            originTransform.position = nextOriginPosition;
        }
        else
        {
            originTransform.SetPositionAndRotation(position, rotation);
        }

        if (characterController != null)
        {
            characterController.enabled = wasControllerEnabled;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        ResolveReferences();

        foreach (var movementBehaviour in _movementBehaviours)
        {
            if (movementBehaviour != null)
            {
                movementBehaviour.enabled = enabled;
            }
        }
    }

    public void SetVisibleReticle(bool visible)
    {
        if (reticleRoot != null)
        {
            reticleRoot.SetActive(visible);
        }
    }

    private void ResolveReferences()
    {
        if (xrOrigin == null)
        {
            xrOrigin = GetComponent<XROrigin>();
            if (xrOrigin == null)
            {
                xrOrigin = GetComponentInChildren<XROrigin>(true);
            }
        }

        ResolveCamera();

        if (characterController == null)
        {
            characterController = GetComponentInChildren<CharacterController>(true);
        }

        _movementBehaviours.Clear();
        AddMovementBehaviour(dynamicMoveProvider);
        AddMovementBehaviour(teleportationProvider);
        AddMovementBehaviour(turnProvider);

        var behaviours = GetComponentsInChildren<Behaviour>(true);
        foreach (var behaviour in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            var typeName = behaviour.GetType().Name;
            if (typeName.Contains("MoveProvider") ||
                typeName.Contains("TurnProvider") ||
                typeName.Contains("TeleportationProvider"))
            {
                AddMovementBehaviour(behaviour);
            }
        }
    }

    private void AddMovementBehaviour(Behaviour movementBehaviour)
    {
        if (movementBehaviour != null && !_movementBehaviours.Contains(movementBehaviour))
        {
            _movementBehaviours.Add(movementBehaviour);
        }
    }

    private Camera ResolveCamera()
    {
        if (xrCamera != null)
        {
            return xrCamera;
        }

        if (xrOrigin != null && xrOrigin.Camera != null)
        {
            xrCamera = xrOrigin.Camera;
        }

        if (xrCamera == null)
        {
            xrCamera = GetComponentInChildren<Camera>(true);
        }

        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }

        return xrCamera;
    }

    private Transform ResolveOriginTransform()
    {
        if (xrOrigin != null)
        {
            return xrOrigin.transform;
        }

        return transform;
    }
}
