using UnityEngine;

[DisallowMultipleComponent]
public class DraggableObject : MonoBehaviour
{
    [SerializeField] private bool autoCreateClickCollider = true;
    [SerializeField] private Vector3 colliderPadding = new Vector3(0.02f, 0.02f, 0.02f);
    [SerializeField] private float releaseSurfaceOffset = 0.005f;
    [SerializeField] private float landingRayStartHeight = 1f;
    [SerializeField] private float landingRayDistance = 5f;
    [SerializeField] private float thrownSettleSpeed = 0.35f;
    [SerializeField] private float thrownSettleAngularSpeed = 1.5f;
    [SerializeField, Range(0f, 1f)] private float supportNormalThreshold = 0.35f;

    private Collider[] _colliders;
    private Rigidbody _rigidbody;
    private float _bottomOffset;
    private bool _shouldSettleAfterThrow;
    private LayerMask _throwSupportLayers;

    public float BottomOffset => _bottomOffset;
    public float ReleaseSurfaceOffset => releaseSurfaceOffset;

    private void Awake()
    {
        EnsureClickCollider();
        EnsureRigidbody();
        RefreshColliders();
        RecalculateBottomOffset();
    }

    private void OnValidate()
    {
        colliderPadding.x = Mathf.Max(0f, colliderPadding.x);
        colliderPadding.y = Mathf.Max(0f, colliderPadding.y);
        colliderPadding.z = Mathf.Max(0f, colliderPadding.z);
        releaseSurfaceOffset = Mathf.Max(0f, releaseSurfaceOffset);
        landingRayStartHeight = Mathf.Max(0.1f, landingRayStartHeight);
        landingRayDistance = Mathf.Max(0.1f, landingRayDistance);
        thrownSettleSpeed = Mathf.Max(0.01f, thrownSettleSpeed);
        thrownSettleAngularSpeed = Mathf.Max(0.01f, thrownSettleAngularSpeed);
        supportNormalThreshold = Mathf.Clamp01(supportNormalThreshold);
    }

    public void BeginDrag()
    {
        EnsureRigidbody();
        RecalculateBottomOffset();
        _shouldSettleAfterThrow = false;
        StopPhysics();
    }

    public void MoveTo(Vector3 position)
    {
        EnsureRigidbody();

        if (_rigidbody != null)
        {
            _rigidbody.MovePosition(position);
            return;
        }

        transform.position = position;
    }

    public void Throw(Vector3 direction, float force, LayerMask supportLayers)
    {
        EnsureRigidbody();
        RecalculateBottomOffset();
        RefreshColliders();
        SetCollidersEnabled(true);

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigidbody.WakeUp();

        _throwSupportLayers = supportLayers;
        _shouldSettleAfterThrow = true;

        var throwDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        _rigidbody.AddForce(throwDirection * Mathf.Max(0f, force), ForceMode.Impulse);
    }

    public bool TryLandBelow(LayerMask supportLayers)
    {
        EnsureRigidbody();
        RecalculateBottomOffset();
        RefreshColliders();
        var bounds = CalculateRendererBounds();
        SetCollidersEnabled(false);

        var castCenter = bounds.HasValue ? bounds.Value.center : transform.position;
        var castBottomY = bounds.HasValue ? bounds.Value.min.y : transform.position.y;
        var castExtents = bounds.HasValue ? bounds.Value.extents : Vector3.one * 0.05f;
        var halfExtents = new Vector3(
            Mathf.Max(0.02f, castExtents.x),
            0.02f,
            Mathf.Max(0.02f, castExtents.z));
        var origin = new Vector3(castCenter.x, castBottomY + landingRayStartHeight, castCenter.z);
        var maxDistance = landingRayStartHeight + landingRayDistance;

        var foundGround = Physics.BoxCast(
            origin,
            halfExtents,
            Vector3.down,
            out var hit,
            Quaternion.identity,
            maxDistance,
            supportLayers,
            QueryTriggerInteraction.Ignore);

        if (foundGround)
        {
            var landedPosition = transform.position;
            landedPosition.y = hit.point.y + _bottomOffset + releaseSurfaceOffset;
            MoveTo(landedPosition);
        }

        SetCollidersEnabled(true);
        StopPhysics();
        return foundGround;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TrySettleAfterThrow(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TrySettleAfterThrow(collision);
    }

    private void SetCollidersEnabled(bool enabled)
    {
        for (var i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null)
            {
                _colliders[i].enabled = enabled;
            }
        }
    }

    private void RecalculateBottomOffset()
    {
        var bounds = CalculateRendererBounds();
        _bottomOffset = bounds.HasValue ? transform.position.y - bounds.Value.min.y : 0f;
    }

    private void EnsureClickCollider()
    {
        if (!autoCreateClickCollider || GetComponent<Collider>() != null)
        {
            return;
        }

        var bounds = CalculateRendererBounds();
        if (!bounds.HasValue)
        {
            return;
        }

        var boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = transform.InverseTransformPoint(bounds.Value.center);

        var localMin = transform.InverseTransformPoint(bounds.Value.min);
        var localMax = transform.InverseTransformPoint(bounds.Value.max);
        var size = new Vector3(
            Mathf.Abs(localMax.x - localMin.x),
            Mathf.Abs(localMax.y - localMin.y),
            Mathf.Abs(localMax.z - localMin.z));
        boxCollider.size = size + colliderPadding;
    }

    private Bounds? CalculateRendererBounds()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return null;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private void EnsureRigidbody()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        if (GetComponent("XRGrabInteractable") != null)
        {
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            return;
        }

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void StopPhysics()
    {
        if (_rigidbody == null)
        {
            return;
        }

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _shouldSettleAfterThrow = false;
    }

    private void RefreshColliders()
    {
        _colliders = GetComponentsInChildren<Collider>();
    }

    private void TrySettleAfterThrow(Collision collision)
    {
        if (!_shouldSettleAfterThrow || _rigidbody == null || _rigidbody.isKinematic)
        {
            return;
        }

        if (!IsLayerInMask(collision.collider.gameObject.layer, _throwSupportLayers) || !HasSupportContact(collision))
        {
            return;
        }

        if (_rigidbody.linearVelocity.sqrMagnitude > thrownSettleSpeed * thrownSettleSpeed)
        {
            return;
        }

        if (_rigidbody.angularVelocity.sqrMagnitude > thrownSettleAngularSpeed * thrownSettleAngularSpeed)
        {
            return;
        }

        TryLandBelow(_throwSupportLayers);
    }

    private bool HasSupportContact(Collision collision)
    {
        for (var i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y >= supportNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
