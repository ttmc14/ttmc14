namespace Content.Shared._MC.Power.Systems.Providing;

/// <summary>
/// Provider for BatteryComponent from Content.Server
/// </summary>
public abstract class MCProvidingSharedBatterySystem : EntitySystem
{
    public virtual bool Supported => false;

    public virtual void SetCharge(EntityUid uid, float value)
    {
    }

    public virtual float GetCharge(EntityUid uid)
    {
        return 0f;
    }

    public virtual float GetMaxCharge(EntityUid uid)
    {
        return 0f;
    }
}
