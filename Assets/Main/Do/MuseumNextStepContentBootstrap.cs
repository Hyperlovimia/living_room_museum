using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class MuseumNextStepContentBootstrap : MonoBehaviour
{
    [SerializeField] private Transform playerController;
    [SerializeField] private Transform spawnTeleportTarget;
    [SerializeField] private bool buildOnStart = true;

    private static readonly ExhibitSeed[] ExhibitSeeds =
    {
        new ExhibitSeed(
            "明式玫瑰椅",
            "玫瑰椅以靠背低矮、扶手平齐为特点，常置于窗边或书斋。它体现了明式家具简洁、含蓄、重比例的审美，也让日常坐具承载文人生活的礼仪感。",
            new Vector3(-13.0f, 0.9f, -5.5f),
            new Vector3(1.25f, 1.2f, 1.25f),
            new Vector3(0f, 0.92f, -0.18f),
            "Media/Pictures/玫瑰椅",
            "Media/Audio/Narration/rose_chair",
            false),
        new ExhibitSeed(
            "茶桌与茶席",
            "茶桌不只是器物摆放的平台，也是待客、静心与交流的场所。茶席中的壶、盏、水声和动线，共同形成中国起居空间里的慢节奏媒介体验。",
            new Vector3(-10.3f, 0.82f, -5.5f),
            new Vector3(1.65f, 0.95f, 1.25f),
            new Vector3(0f, 0.78f, -0.15f),
            "Media/Pictures/茶桌1",
            "Media/Audio/Narration/tea_table",
            false),
        new ExhibitSeed(
            "文房书案",
            "书案连接书写、阅读与收藏。笔墨纸砚的陈设让家具从功能物转为文化媒介，展示古人如何在起居空间中组织知识与审美。",
            new Vector3(-7.6f, 0.86f, -5.5f),
            new Vector3(1.8f, 1.05f, 1.2f),
            new Vector3(0f, 0.82f, -0.16f),
            null,
            "Media/Audio/Narration/writing_desk",
            false),
        new ExhibitSeed(
            "插屏与空间分隔",
            "屏风既可遮挡视线，也可组织空间层次。它让厅堂、书房与茶室之间形成含蓄的边界，是传统室内设计中兼具实用与审美的媒介。",
            new Vector3(-4.9f, 1.05f, -5.5f),
            new Vector3(1.45f, 1.6f, 0.8f),
            new Vector3(0f, 1.05f, -0.12f),
            null,
            "Media/Audio/Narration/screen",
            false),
        new ExhibitSeed(
            "起居文化视频讲解",
            "这一段视频式讲解概括本展厅的核心：家具、光线、器物与行动路线共同构成起居文化。用户通过点击、聆听、寻找与传送，完成一次互动式空间阅读。",
            new Vector3(-2.2f, 1.05f, -5.5f),
            new Vector3(1.45f, 1.5f, 0.8f),
            new Vector3(0f, 1.0f, -0.12f),
            null,
            null,
            true)
    };

    private void Start()
    {
        if (!buildOnStart)
        {
            return;
        }

        ResolveReferences();
        BuildExhibitHotspots();
        BuildReturnButton();
        EnsureWelcomeSequence();
    }

    [ContextMenu("Build Next Step Content")]
    private void BuildNow()
    {
        ResolveReferences();
        BuildExhibitHotspots();
        BuildReturnButton();
        EnsureWelcomeSequence();
    }

    private void ResolveReferences()
    {
        if (playerController == null)
        {
            var player = GameObject.Find("PlayerController");
            if (player != null)
            {
                playerController = player.transform;
            }
        }

        if (spawnTeleportTarget == null)
        {
            var target = GameObject.Find("出生点位置");
            if (target != null)
            {
                spawnTeleportTarget = target.transform;
            }
        }
    }

    private void BuildExhibitHotspots()
    {
        var root = GetOrCreateRoot("下一步展品热点");
        for (var i = 0; i < ExhibitSeeds.Length; i++)
        {
            var seed = ExhibitSeeds[i];
            var objectName = $"展品热点_{i + 1}_{seed.Title}";
            if (GameObject.Find(objectName) != null)
            {
                continue;
            }

            var exhibit = new GameObject(objectName);
            exhibit.name = objectName;
            exhibit.transform.SetParent(root.transform, false);
            exhibit.transform.position = seed.Position;

            var collider = exhibit.AddComponent<BoxCollider>();
            collider.size = seed.HitboxSize;
            collider.isTrigger = false;

            var info = exhibit.AddComponent<ExhibitInfo>();
            var texture = string.IsNullOrEmpty(seed.PictureResource) ? null : Resources.Load<Texture>(seed.PictureResource);
            var audioClip = string.IsNullOrEmpty(seed.AudioResource) ? null : Resources.Load<AudioClip>(seed.AudioResource);
            var videoClip = Resources.Load<VideoClip>("Media/Video/中华起居文化欢迎视频");
            info.Configure(
                seed.Title,
                seed.Description,
                texture,
                audioClip,
                seed.ScriptedVideo ? videoClip : null,
                true,
                seed.ScriptedVideo && videoClip == null,
                "视频讲解：中华起居文化由器物、行动和空间共同构成。请继续探索展厅，完成找一找挑战。");

            CreateSubtleMarker(exhibit.transform, seed.Title, seed.MarkerOffset);
        }
    }

    private void BuildReturnButton()
    {
        if (GameObject.Find("书房返回大厅按钮") != null)
        {
            return;
        }

        var root = GetOrCreateRoot("下一步展品热点");
        var button = new GameObject("书房返回大厅按钮");
        button.name = "书房返回大厅按钮";
        button.transform.SetParent(root.transform, false);
        button.transform.position = new Vector3(-9.45f, 1.05f, 1.35f);

        var collider = button.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.65f, 0.7f, 0.35f);
        collider.isTrigger = false;
        CreateReturnButtonVisual(button.transform);

        var teleport = button.AddComponent<TeleportClickInteractable>();
        teleport.Configure(playerController, spawnTeleportTarget, "返回大厅");
    }

    private void EnsureWelcomeSequence()
    {
        if (GetComponent<WelcomeSequenceController>() != null)
        {
            return;
        }

        var welcome = gameObject.AddComponent<WelcomeSequenceController>();
        var audioClip = Resources.Load<AudioClip>("Media/Audio/Narration/welcome");
        welcome.Configure(playerController, audioClip);
    }

    private static GameObject GetOrCreateRoot(string rootName)
    {
        var root = GameObject.Find(rootName);
        if (root != null)
        {
            return root;
        }

        root = new GameObject(rootName);
        return root;
    }

    private static void CreateSubtleMarker(Transform parent, string label, Vector3 localOffset)
    {
        var font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "PingFang SC", "Arial" }, 16);
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        var canvasGo = new GameObject("ExhibitMarker", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(parent, false);
        canvasGo.transform.localPosition = localOffset;
        canvasGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.0065f;

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rect = canvasGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150f, 52f);

        var background = canvasGo.AddComponent<Image>();
        background.color = new Color(0.08f, 0.065f, 0.052f, 0.72f);
        background.raycastTarget = false;

        var accentGo = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accentGo.transform.SetParent(canvasGo.transform, false);
        var accentRect = accentGo.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(5f, 0f);
        var accent = accentGo.GetComponent<Image>();
        accent.color = new Color(0.86f, 0.68f, 0.42f, 0.95f);
        accent.raycastTarget = false;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(UnityEngine.UI.Text));
        textGo.transform.SetParent(canvasGo.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 5f);
        textRect.offsetMax = new Vector2(-8f, -5f);

        var text = textGo.GetComponent<UnityEngine.UI.Text>();
        text.font = font;
        text.text = label + "\n点击查看";
        text.fontSize = 13;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleLeft;
        text.lineSpacing = 0.86f;
        text.color = new Color(1f, 0.94f, 0.78f, 1f);
        text.raycastTarget = false;
    }

    private static void CreateReturnButtonVisual(Transform parent)
    {
        var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "返回大厅木牌";
        board.transform.SetParent(parent, false);
        board.transform.localPosition = Vector3.zero;
        board.transform.localScale = new Vector3(1.58f, 0.1f, 0.48f);

        var boardCollider = board.GetComponent<Collider>();
        if (boardCollider != null)
        {
            Destroy(boardCollider);
        }

        var renderer = board.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.23f, 0.13f, 0.07f, 1f);
        }

        CreateTrim(parent, "上沿", new Vector3(0f, 0.065f, 0.25f));
        CreateTrim(parent, "下沿", new Vector3(0f, 0.065f, -0.25f));
    }

    private static void CreateTrim(Transform parent, string name, Vector3 localPosition)
    {
        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = name;
        trim.transform.SetParent(parent, false);
        trim.transform.localPosition = localPosition;
        trim.transform.localScale = new Vector3(1.62f, 0.035f, 0.035f);

        var collider = trim.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var renderer = trim.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = new Color(0.78f, 0.58f, 0.26f, 1f);
        }
    }

    private readonly struct ExhibitSeed
    {
        public readonly string Title;
        public readonly string Description;
        public readonly Vector3 Position;
        public readonly Vector3 HitboxSize;
        public readonly Vector3 MarkerOffset;
        public readonly string PictureResource;
        public readonly string AudioResource;
        public readonly bool ScriptedVideo;

        public ExhibitSeed(
            string title,
            string description,
            Vector3 position,
            Vector3 hitboxSize,
            Vector3 markerOffset,
            string pictureResource,
            string audioResource,
            bool scriptedVideo)
        {
            Title = title;
            Description = description;
            Position = position;
            HitboxSize = hitboxSize;
            MarkerOffset = markerOffset;
            PictureResource = pictureResource;
            AudioResource = audioResource;
            ScriptedVideo = scriptedVideo;
        }
    }
}
