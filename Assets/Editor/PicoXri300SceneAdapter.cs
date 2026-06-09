using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class PicoXri300SceneAdapter
{
    private const string MenuPath = "Tools/PICO/Apply XRI300 Character Control Adaptation";
    private const string Xri300RootName = "[Building Block] XR Origin (XR Rig) XRI300";
    private const string XrOriginPath = "[Building Block] XR Origin (XR Rig) XRI300/XR Origin (XR Rig)";
    private const string XrMovePath = "[Building Block] XR Origin (XR Rig) XRI300/XR Origin (XR Rig)/Locomotion/Move";
    private const string OldPlayerRootPath = "PlayerController";
    private const string OldPlayerCapsulePath = "PlayerController/PlayerCapsule";
    private const string OldPlayerCameraPath = "PlayerController/MainCamera";
    private const string OldPlayerFollowCameraPath = "PlayerController/PlayerFollowCamera";

    [MenuItem(MenuPath)]
    public static void Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("No active scene is loaded.");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Apply PICO XRI300 Character Control Adaptation");

        EnableXri300Rig();
        EnsureInteractionManager();
        EnsureEventSystem();
        DisableLegacyPlayerRuntime();
        EnsurePlayerAdapter();
        EnsureWorldCanvasRaycasters();
        EnsureSelectableInteractables();
        EnsureGrabInteractables();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("PICO XRI300 character control adaptation applied to " + scene.path);
    }

    private static void EnableXri300Rig()
    {
        var root = FindSceneObjectByPath(Xri300RootName);
        if (root != null)
        {
            Undo.RecordObject(root, "Enable XRI300 root");
            root.SetActive(true);
        }

        var xrOrigin = FindSceneObjectByPath(XrOriginPath);
        if (xrOrigin != null)
        {
            Undo.RecordObject(xrOrigin, "Enable XRI300 origin");
            xrOrigin.SetActive(true);
        }

        var move = FindSceneObjectByPath(XrMovePath);
        if (move != null)
        {
            Undo.RecordObject(move, "Enable continuous move provider");
            move.SetActive(true);
        }
    }

    private static void EnsureInteractionManager()
    {
        var managers = UnityEngine.Object.FindObjectsByType<XRInteractionManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (managers.Length > 0)
        {
            return;
        }

        var managerObject = new GameObject("XR Interaction Manager");
        Undo.RegisterCreatedObjectUndo(managerObject, "Create XR Interaction Manager");
        managerObject.AddComponent<XRInteractionManager>();
    }

    private static void EnsureEventSystem()
    {
        var eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("UI_EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create XR UI EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        var standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneInputModule != null)
        {
            Undo.DestroyObjectImmediate(standaloneInputModule);
        }

        var inputSystemUiModule = eventSystem.GetComponents<Behaviour>()
            .FirstOrDefault(behaviour => behaviour.GetType().Name == "InputSystemUIInputModule");
        if (inputSystemUiModule != null)
        {
            Undo.DestroyObjectImmediate(inputSystemUiModule);
        }

        if (eventSystem.GetComponent<XRUIInputModule>() == null)
        {
            Undo.AddComponent<XRUIInputModule>(eventSystem.gameObject);
        }
    }

    private static void DisableLegacyPlayerRuntime()
    {
        var oldPlayer = FindSceneObjectByPath(OldPlayerRootPath);
        if (oldPlayer != null)
        {
            SetNamedBehavioursEnabled(oldPlayer, false, "MouseDragPickupController", "PlayerMovementBounds");
            oldPlayer.SetActive(true);
        }

        var capsule = FindSceneObjectByPath(OldPlayerCapsulePath);
        if (capsule != null)
        {
            Undo.RecordObject(capsule, "Disable legacy player capsule");
            capsule.SetActive(false);
            SetNamedBehavioursEnabled(capsule, false, "FirstPersonController", "StarterAssetsInputs", "PlayerInput", "BasicRigidBodyPush");
        }

        var oldCamera = FindSceneObjectByPath(OldPlayerCameraPath);
        if (oldCamera != null)
        {
            Undo.RecordObject(oldCamera, "Disable legacy player camera");
            oldCamera.SetActive(false);
            SetNamedBehavioursEnabled(oldCamera, false, "Camera", "AudioListener", "CinemachineBrain");
        }

        var followCamera = FindSceneObjectByPath(OldPlayerFollowCameraPath);
        if (followCamera != null)
        {
            Undo.RecordObject(followCamera, "Disable legacy follow camera");
            followCamera.SetActive(false);
            SetNamedBehavioursEnabled(followCamera, false, "CinemachineVirtualCamera");
        }
    }

    private static void EnsurePlayerAdapter()
    {
        var xrOriginObject = FindSceneObjectByPath(XrOriginPath);
        if (xrOriginObject == null)
        {
            Debug.LogWarning("XRI300 XR Origin was not found; XROriginPlayerAdapter was not added.");
            return;
        }

        var adapter = xrOriginObject.GetComponent<XROriginPlayerAdapter>();
        if (adapter == null)
        {
            adapter = Undo.AddComponent<XROriginPlayerAdapter>(xrOriginObject);
        }

        var serializedAdapter = new SerializedObject(adapter);
        SetObject(serializedAdapter, "xrOrigin", xrOriginObject.GetComponent<XROrigin>());
        SetObject(serializedAdapter, "xrCamera", xrOriginObject.GetComponentInChildren<Camera>(true));
        SetObject(serializedAdapter, "dynamicMoveProvider", FindSceneObjectByPath(XrMovePath)?.GetComponent<Behaviour>());
        SetObject(serializedAdapter, "teleportationProvider", FindBehaviourByTypeName(xrOriginObject, "TeleportationProvider"));
        SetObject(serializedAdapter, "turnProvider", FindBehaviourByTypeName(xrOriginObject, "SnapTurnProvider"));
        SetObject(serializedAdapter, "characterController", xrOriginObject.GetComponent<CharacterController>());
        serializedAdapter.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureWorldCanvasRaycasters()
    {
        var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                continue;
            }

            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            {
                Undo.AddComponent<TrackedDeviceGraphicRaycaster>(canvas.gameObject);
            }
        }
    }

    private static void EnsureSelectableInteractables()
    {
        foreach (var selectable in EnumerateSelectableBehaviours())
        {
            var behaviour = selectable as MonoBehaviour;
            if (behaviour == null || behaviour is ExhibitInteractionController)
            {
                continue;
            }

            EnsureColliderForInteraction(behaviour.gameObject);
            EnsureSimpleInteractable(behaviour);
        }
    }

    private static void EnsureGrabInteractables()
    {
        var draggables = UnityEngine.Object.FindObjectsByType<DraggableObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var draggable in draggables)
        {
            var gameObject = draggable.gameObject;
            EnsureColliderForInteraction(gameObject);
            var colliders = gameObject.GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                Undo.RecordObject(collider, "Configure grab collider");
                collider.isTrigger = false;
            }

            var rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = Undo.AddComponent<Rigidbody>(gameObject);
            }

            Undo.RecordObject(rigidbody, "Configure XR grab Rigidbody");
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var grabInteractable = gameObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = Undo.AddComponent<XRGrabInteractable>(gameObject);
            }

            Undo.RecordObject(grabInteractable, "Configure XR grab interactable");
            grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            grabInteractable.throwOnDetach = true;
        }
    }

    private static IEnumerable<IXrSelectable> EnumerateSelectableBehaviours()
    {
        var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IXrSelectable selectable)
            {
                yield return selectable;
            }
        }
    }

    private static void EnsureSimpleInteractable(MonoBehaviour selectableBehaviour)
    {
        var gameObject = selectableBehaviour.gameObject;
        var simpleInteractable = gameObject.GetComponent<XRSimpleInteractable>();
        if (simpleInteractable == null)
        {
            simpleInteractable = Undo.AddComponent<XRSimpleInteractable>(gameObject);
        }

        var bridge = gameObject.GetComponent<XrSelectableBridge>();
        if (bridge == null)
        {
            bridge = Undo.AddComponent<XrSelectableBridge>(gameObject);
        }

        var serializedBridge = new SerializedObject(bridge);
        SetObject(serializedBridge, "target", selectableBehaviour);
        SetBool(serializedBridge, "searchParents", true);
        SetBool(serializedBridge, "searchChildren", true);
        serializedBridge.ApplyModifiedPropertiesWithoutUndo();

        var colliders = gameObject.GetComponentsInChildren<Collider>(true);
        foreach (var collider in colliders)
        {
            Undo.RecordObject(collider, "Configure XR select collider");
            collider.isTrigger = false;
        }
    }

    private static void EnsureColliderForInteraction(GameObject gameObject)
    {
        if (gameObject.GetComponentInChildren<Collider>(true) != null)
        {
            return;
        }

        var bounds = CalculateRendererBounds(gameObject);
        if (!bounds.HasValue)
        {
            return;
        }

        var boxCollider = Undo.AddComponent<BoxCollider>(gameObject);
        var transform = gameObject.transform;
        boxCollider.center = transform.InverseTransformPoint(bounds.Value.center);
        var localMin = transform.InverseTransformPoint(bounds.Value.min);
        var localMax = transform.InverseTransformPoint(bounds.Value.max);
        boxCollider.size = new Vector3(
            Mathf.Abs(localMax.x - localMin.x),
            Mathf.Abs(localMax.y - localMin.y),
            Mathf.Abs(localMax.z - localMin.z));
    }

    private static Bounds? CalculateRendererBounds(GameObject gameObject)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
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

    private static void SetNamedBehavioursEnabled(GameObject root, bool enabled, params string[] typeNames)
    {
        var names = new HashSet<string>(typeNames);
        var behaviours = root.GetComponentsInChildren<Behaviour>(true);
        foreach (var behaviour in behaviours)
        {
            if (!names.Contains(behaviour.GetType().Name))
            {
                continue;
            }

            Undo.RecordObject(behaviour, "Toggle legacy behaviour");
            behaviour.enabled = enabled;
        }
    }

    private static Behaviour FindBehaviourByTypeName(GameObject root, string typeName)
    {
        if (root == null)
        {
            return null;
        }

        return root.GetComponentsInChildren<Behaviour>(true)
            .FirstOrDefault(behaviour => behaviour.GetType().Name == typeName);
    }

    private static GameObject FindSceneObjectByPath(string path)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == path)
            {
                return root;
            }

            var found = FindChildByPath(root.transform, path);
            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChildByPath(Transform root, string path)
    {
        var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts[0] != root.name)
        {
            return null;
        }

        var current = root;
        for (var i = 1; i < parts.Length; i++)
        {
            current = FindDirectChild(current, parts[i]);
            if (current == null)
            {
                return null;
            }
        }

        return current;
    }

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }
}
