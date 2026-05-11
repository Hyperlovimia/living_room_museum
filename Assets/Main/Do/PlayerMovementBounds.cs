using UnityEngine;

/// <summary>
/// 将玩家限制在一个世界空间立方体区域内，而不依赖额外碰撞体。
/// </summary>
public class PlayerMovementBounds : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private CharacterController targetController;
    [SerializeField] private Vector3 boundsCenter = new Vector3(-5f, 0f, -2f);
    [SerializeField] private Vector3 boundsSize = new Vector3(20f, 6f, 20f);

    private const float MinSize = 0.01f;

    private void Reset()
    {
        TryAutoAssign();
        if (target != null)
        {
            boundsCenter = target.position;
        }
    }

    private void Awake()
    {
        TryAutoAssign();
    }

    private void OnValidate()
    {
        boundsSize.x = Mathf.Max(boundsSize.x, MinSize);
        boundsSize.y = Mathf.Max(boundsSize.y, MinSize);
        boundsSize.z = Mathf.Max(boundsSize.z, MinSize);

        if (!Application.isPlaying)
        {
            TryAutoAssign();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 halfSize = boundsSize * 0.5f;
        Vector3 min = boundsCenter - halfSize;
        Vector3 max = boundsCenter + halfSize;
        Vector3 currentPosition = target.position;

        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(currentPosition.x, min.x, max.x),
            Mathf.Clamp(currentPosition.y, min.y, max.y),
            Mathf.Clamp(currentPosition.z, min.z, max.z));

        if ((clampedPosition - currentPosition).sqrMagnitude <= 0.000001f)
        {
            return;
        }

        if (targetController != null && targetController.enabled)
        {
            targetController.enabled = false;
            target.position = clampedPosition;
            targetController.enabled = true;
            return;
        }

        target.position = clampedPosition;
    }

    private void TryAutoAssign()
    {
        if (targetController == null)
        {
            targetController = GetComponentInChildren<CharacterController>();
        }

        if (target == null)
        {
            target = targetController != null ? targetController.transform : transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.25f);
        Gizmos.DrawCube(boundsCenter, boundsSize);
        Gizmos.color = new Color(0f, 0.7f, 1f, 1f);
        Gizmos.DrawWireCube(boundsCenter, boundsSize);
    }
}
