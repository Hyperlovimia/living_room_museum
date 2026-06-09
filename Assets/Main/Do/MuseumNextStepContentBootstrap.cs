using UnityEngine;

[DisallowMultipleComponent]
public class MuseumNextStepContentBootstrap : MonoBehaviour
{
    [SerializeField] private Transform playerController;
    [SerializeField] private Transform spawnTeleportTarget;
    [SerializeField] private bool buildOnStart = true;

    private void Start()
    {
        if (!buildOnStart)
        {
            return;
        }

        ResolveReferences();
        BuildReturnButton();
        EnsureWelcomeSequence();
    }

    [ContextMenu("Build Next Step Content")]
    private void BuildNow()
    {
        ResolveReferences();
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
            var spawnRoom = FindAnyObjectByType<SpawnRoomController>();
            if (spawnRoom != null && spawnRoom.spawnTeleportTarget != null)
            {
                spawnTeleportTarget = spawnRoom.spawnTeleportTarget;
            }
            else
            {
                var target = GameObject.Find("出生点位置");
                if (target != null)
                {
                    spawnTeleportTarget = target.transform;
                }
            }
        }
    }

    private void BuildReturnButton()
    {
        var existingButton = GameObject.Find("书房返回大厅按钮");
        if (existingButton != null)
        {
            ConfigureReturnButton(existingButton);
            return;
        }

        var root = GetOrCreateRoot("书房交互控件");
        var button = new GameObject("书房返回大厅按钮");
        button.name = "书房返回大厅按钮";
        button.transform.SetParent(root.transform, false);
        button.transform.position = new Vector3(-9.45f, 1.05f, 1.35f);

        var collider = button.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.65f, 0.7f, 0.35f);
        collider.isTrigger = false;
        CreateReturnButtonVisual(button.transform);

        ConfigureReturnButton(button);
    }

    private void ConfigureReturnButton(GameObject button)
    {
        var teleport = button.GetComponent<TeleportClickInteractable>();
        if (teleport == null)
        {
            teleport = button.AddComponent<TeleportClickInteractable>();
        }

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

}
