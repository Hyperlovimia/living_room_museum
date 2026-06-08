using UnityEngine;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class ExhibitInfo : MonoBehaviour
{
    [SerializeField] private string exhibitTitle = "Exhibit";
    [TextArea(4, 10)]
    [SerializeField] private string description = "Description";
    [SerializeField] private Texture displayTexture;
    [SerializeField] private AudioClip narrationClip;
    [SerializeField] private VideoClip exhibitVideo;
    [SerializeField] private bool loopVideo = true;
    [SerializeField] private bool scriptedVideoEnabled;
    [TextArea(2, 6)]
    [SerializeField] private string scriptedVideoCaption;
    [SerializeField] private bool autoCreateCollider = true;

    private Texture _resolvedFallbackTexture;
    private bool _hasResolvedFallbackTexture;

    public string ExhibitTitle => string.IsNullOrWhiteSpace(exhibitTitle) ? gameObject.name : exhibitTitle;
    public string Description => description;
    public Texture DisplayTexture => displayTexture != null ? displayTexture : ResolveFallbackTexture();
    public AudioClip NarrationClip => narrationClip;
    public VideoClip ExhibitVideo => exhibitVideo;
    public bool LoopVideo => loopVideo;
    public bool ScriptedVideoEnabled => scriptedVideoEnabled;
    public string ScriptedVideoCaption => scriptedVideoCaption;

    public void Configure(
        string title,
        string body,
        Texture texture,
        AudioClip audioClip,
        VideoClip videoClip = null,
        bool loop = true,
        bool scriptedVideo = false,
        string scriptedCaption = null)
    {
        exhibitTitle = title;
        description = body;
        displayTexture = texture;
        narrationClip = audioClip;
        exhibitVideo = videoClip;
        loopVideo = loop;
        scriptedVideoEnabled = scriptedVideo;
        scriptedVideoCaption = scriptedCaption;
    }

    private void Awake()
    {
        if (autoCreateCollider && GetComponent<Collider>() == null)
        {
            FitColliderToChildren();
        }
    }

    [ContextMenu("Fit Box Collider To Children")]
    private void FitColliderToChildren()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        var hasBounds = false;
        var localBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var renderer in renderers)
        {
            var bounds = renderer.bounds;
            foreach (var corner in GetBoundsCorners(bounds))
            {
                var localPoint = transform.InverseTransformPoint(corner);
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

        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.center = localBounds.center;
        boxCollider.size = localBounds.size;
    }

    private Texture ResolveFallbackTexture()
    {
        if (_hasResolvedFallbackTexture)
        {
            return _resolvedFallbackTexture;
        }

        _hasResolvedFallbackTexture = true;
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            var material = renderer.sharedMaterial;
            if (material != null && material.mainTexture != null)
            {
                _resolvedFallbackTexture = material.mainTexture;
                break;
            }
        }

        return _resolvedFallbackTexture;
    }

    private static Vector3[] GetBoundsCorners(Bounds bounds)
    {
        var min = bounds.min;
        var max = bounds.max;
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
}
