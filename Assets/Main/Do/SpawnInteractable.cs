using UnityEngine;

[DisallowMultipleComponent]
public class SpawnInteractable : MonoBehaviour, IXrSelectable
{
    public string label = "Button";

    public void Select(XrSelectContext context)
    {
        var controller = FindFirstObjectByType<SpawnRoomController>();
        if (controller != null)
        {
            controller.HandleInteractable(this);
        }
    }
}
