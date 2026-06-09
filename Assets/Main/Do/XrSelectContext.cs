using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public readonly struct XrSelectContext
{
    public readonly IXRSelectInteractor Interactor;
    public readonly IXRSelectInteractable Interactable;
    public readonly GameObject Target;
    public readonly RaycastHit Hit;
    public readonly bool HasHit;
    public readonly bool FromUi;
    public readonly bool FromMouseFallback;

    public XrSelectContext(
        IXRSelectInteractor interactor,
        IXRSelectInteractable interactable,
        GameObject target,
        bool fromUi,
        bool fromMouseFallback,
        RaycastHit hit = default,
        bool hasHit = false)
    {
        Interactor = interactor;
        Interactable = interactable;
        Target = target;
        FromUi = fromUi;
        FromMouseFallback = fromMouseFallback;
        Hit = hit;
        HasHit = hasHit;
    }

    public static XrSelectContext FromXr(SelectEnterEventArgs args, GameObject target)
    {
        return new XrSelectContext(
            args?.interactorObject,
            args?.interactableObject,
            target,
            false,
            false);
    }

    public static XrSelectContext MouseFallback(GameObject target, RaycastHit hit = default, bool hasHit = false)
    {
        return new XrSelectContext(null, null, target, false, true, hit, hasHit);
    }
}
