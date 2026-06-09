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
    [SerializeField] private bool autoCreateCollider;

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
        if (autoCreateCollider && GetComponentInChildren<Collider>(true) == null)
        {
            AddMeshCollidersToChildren();
        }
    }

    [ContextMenu("Add Mesh Colliders To Children")]
    private void AddMeshCollidersToChildren()
    {
        var meshFilters = GetComponentsInChildren<MeshFilter>(true);
        foreach (var meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null || meshFilter.GetComponent<Collider>() != null)
            {
                continue;
            }

            if (meshFilter.GetComponent<Renderer>() == null)
            {
                continue;
            }

            var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
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
}
