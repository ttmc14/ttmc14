using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._MC.Power.Systems.Providing;

namespace Content.Server._MC.Power.Systems.Providing;

public sealed class MCProvidingBatterySystem : MCProvidingSharedBatterySystem
{
    [Dependency] private readonly BatterySystem _battery = null!;

    private EntityQuery<BatteryComponent> _batteryQuery;

    public override bool Supported => true;

    public override void Initialize()
    {
        base.Initialize();

        _batteryQuery = GetEntityQuery<BatteryComponent>();
    }

    public override void SetCharge(EntityUid uid, float value)
    {
        _battery.SetCharge(uid, value);
    }

    public override float GetCharge(EntityUid uid)
    {
        return _batteryQuery.TryGetComponent(uid, out var component)
            ? component.CurrentCharge
            : 0f;
    }

    public override float GetMaxCharge(EntityUid uid)
    {
        return _batteryQuery.TryGetComponent(uid, out var component)
            ? component.MaxCharge
            : 0f;
    }
}
