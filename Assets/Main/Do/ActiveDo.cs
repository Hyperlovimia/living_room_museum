using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class ActiveDo : DoBase
{
    private enum LightState
    {
        Off,
        Dim,
        Bright
    }

    public bool IsActive;

    public Transform Target;

    public float dimIntensity = 0.5f;

    public float brightIntensity = 1f;

    private Light targetLight;

    private LightState currentLightState;

    private void Awake()
    {
        if (Target == null)
        {
            return;
        }

        targetLight = Target.GetComponent<Light>();
        if (targetLight != null)
        {
            currentLightState = GetCurrentLightState();
            ApplyLightState(currentLightState);
            return;
        }

        Target.gameObject.SetActive(IsActive);
    }

    private void OnMouseDown()
    {
        if (IsPointerOverUI()) return;

        if (Target == null)
        {
            return;
        }

        if (targetLight != null)
        {
            currentLightState = GetNextLightState(currentLightState);
            ApplyLightState(currentLightState);
            return;
        }

        IsActive = !IsActive;
        Target.gameObject.SetActive(IsActive);
    }

    private LightState GetCurrentLightState()
    {
        if (!targetLight.enabled)
        {
            return LightState.Off;
        }

        float dimDifference = Mathf.Abs(targetLight.intensity - dimIntensity);
        float brightDifference = Mathf.Abs(targetLight.intensity - brightIntensity);
        return dimDifference <= brightDifference ? LightState.Dim : LightState.Bright;
    }

    private LightState GetNextLightState(LightState state)
    {
        switch (state)
        {
            case LightState.Off:
                return LightState.Dim;
            case LightState.Dim:
                return LightState.Bright;
            default:
                return LightState.Off;
        }
    }

    private void ApplyLightState(LightState state)
    {
        switch (state)
        {
            case LightState.Off:
                targetLight.enabled = false;
                IsActive = false;
                break;
            case LightState.Dim:
                targetLight.enabled = true;
                targetLight.intensity = dimIntensity;
                IsActive = true;
                break;
            default:
                targetLight.enabled = true;
                targetLight.intensity = brightIntensity;
                IsActive = true;
                break;
        }
    }
}
