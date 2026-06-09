using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class ActiveDo : DoBase
{
    public enum LightState
    {
        Off,
        Dim,
        Bright
    }

    public enum LightingMode
    {
        MorningReading,
        GuestMode,
        TeaMode
    }

    public bool IsActive;

    public Transform Target;

    [Header("Light Intensities")]
    public float dimIntensity = 0.5f;

    public float brightIntensity = 1f;

    [Header("Lighting Presets")]
    public LightState morningReadingState = LightState.Dim;

    public LightState guestModeState = LightState.Bright;

    public LightState teaModeState = LightState.Off;

    private Light targetLight;

    private LightState currentLightState;

    private void Awake()
    {
        if (Target == null)
        {
            return;
        }

        if (TryGetTargetLight())
        {
            currentLightState = GetCurrentLightState();
            ApplyLightState(currentLightState);
            return;
        }

        Target.gameObject.SetActive(IsActive);
    }

    protected override void OnSelected(XrSelectContext context)
    {
        if (Target == null)
        {
            return;
        }

        if (TryGetTargetLight())
        {
            currentLightState = GetNextLightState(currentLightState);
            ApplyLightState(currentLightState);
            return;
        }

        IsActive = !IsActive;
        Target.gameObject.SetActive(IsActive);
    }

    public bool SupportsLightingModes()
    {
        return TryGetTargetLight();
    }

    public void ApplyLightingMode(LightingMode lightingMode)
    {
        if (!TryGetTargetLight())
        {
            return;
        }

        currentLightState = GetConfiguredLightState(lightingMode);
        ApplyLightState(currentLightState);
    }

    private bool TryGetTargetLight()
    {
        if (Target == null)
        {
            return false;
        }

        if (targetLight == null)
        {
            targetLight = Target.GetComponent<Light>();
        }

        return targetLight != null;
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

    private LightState GetConfiguredLightState(LightingMode lightingMode)
    {
        switch (lightingMode)
        {
            case LightingMode.MorningReading:
                return morningReadingState;
            case LightingMode.GuestMode:
                return guestModeState;
            default:
                return teaModeState;
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
