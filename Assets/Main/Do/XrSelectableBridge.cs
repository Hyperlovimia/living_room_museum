using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
[RequireComponent(typeof(XRSimpleInteractable))]
public class XrSelectableBridge : MonoBehaviour
{
    [SerializeField] private MonoBehaviour target;
    [SerializeField] private bool searchParents = true;
    [SerializeField] private bool searchChildren = false;

    private XRSimpleInteractable _interactable;
    private IXrSelectable _selectable;

    private void Awake()
    {
        Resolve();
    }

    private void OnEnable()
    {
        Resolve();
        _interactable.selectEntered.AddListener(SelectFromXr);
    }

    private void OnDisable()
    {
        if (_interactable != null)
        {
            _interactable.selectEntered.RemoveListener(SelectFromXr);
        }
    }

    public void SetTarget(MonoBehaviour selectableTarget)
    {
        target = selectableTarget;
        _selectable = selectableTarget as IXrSelectable;
    }

    public void SelectFromXr(SelectEnterEventArgs args)
    {
        Resolve();
        if (_selectable == null)
        {
            return;
        }

        _selectable.Select(XrSelectContext.FromXr(args, gameObject));
    }

    private void Resolve()
    {
        if (_interactable == null)
        {
            _interactable = GetComponent<XRSimpleInteractable>();
        }

        _selectable = target as IXrSelectable;
        if (_selectable != null)
        {
            return;
        }

        _selectable = FindSelectableOn(gameObject);
        if (_selectable != null)
        {
            target = _selectable as MonoBehaviour;
            return;
        }

        if (searchParents)
        {
            _selectable = FindSelectableInParents();
            if (_selectable != null)
            {
                target = _selectable as MonoBehaviour;
                return;
            }
        }

        if (searchChildren)
        {
            _selectable = FindSelectableInChildren();
            if (_selectable != null)
            {
                target = _selectable as MonoBehaviour;
            }
        }
    }

    private IXrSelectable FindSelectableInParents()
    {
        var current = transform.parent;
        while (current != null)
        {
            var selectable = FindSelectableOn(current.gameObject);
            if (selectable != null)
            {
                return selectable;
            }

            current = current.parent;
        }

        return null;
    }

    private IXrSelectable FindSelectableInChildren()
    {
        var behaviours = GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var behaviour in behaviours)
        {
            if (behaviour != this && behaviour is IXrSelectable selectable)
            {
                return selectable;
            }
        }

        return null;
    }

    private IXrSelectable FindSelectableOn(GameObject owner)
    {
        var behaviours = owner.GetComponents<MonoBehaviour>();
        foreach (var behaviour in behaviours)
        {
            if (behaviour != this && behaviour is IXrSelectable selectable)
            {
                return selectable;
            }
        }

        return null;
    }
}
