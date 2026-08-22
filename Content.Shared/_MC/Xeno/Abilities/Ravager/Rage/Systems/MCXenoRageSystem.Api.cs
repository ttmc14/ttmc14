using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Events;
using JetBrains.Annotations;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems;

public sealed partial class MCXenoRageSystem
{
    [PublicAPI]
    public float GetPower(EntityUid uid)
    {
        if (!_query.HasComp(uid) || !_queryActive.TryComp(uid, out var component))
            return 0;

        return component.Power;
    }

    [PublicAPI]
    public void SetPower(EntityUid uid, float value)
    {
        if (!_query.HasComp(uid) || !_queryActive.TryComp(uid, out var component))
            return;

        var power = component.Power;
        component.Power = value;

        var ev = new MCXenoRagePowerChangedEvent(value, power);
        RaiseLocalEvent(uid, ref ev);
    }

    [PublicAPI]
    public void ResetPower(EntityUid uid)
    {
        SetPower(uid, 0);
    }

    [PublicAPI]
    public bool IsActive(EntityUid uid)
    {
        return HasComp<MCXenoRageActiveComponent>(uid);
    }

    [PublicAPI]
    public void Activate(EntityUid uid)
    {
        if (!_query.HasComp(uid))
            return;

        var ev = new MCXenoRageActivateEvent();
        RaiseLocalEvent(uid, ref ev);

        EnsureComp<MCXenoRageActiveComponent>(uid);
    }

    [PublicAPI]
    public void Deactivate(EntityUid uid)
    {
        DeactivateInternal(uid);
        RemComp<MCXenoRageActiveComponent>(uid);
    }

    [PublicAPI]
    public void DeactivateDeferred(EntityUid uid)
    {
        DeactivateInternal(uid);
        RemCompDeferred<MCXenoRageActiveComponent>(uid);
    }

    [PublicAPI]
    public MCXenoRageComponent GetConfiguration(Entity<MCXenoRageActiveComponent> entity)
    {
        return EnsureComp<MCXenoRageComponent>(entity.Owner);
    }

    private void DeactivateInternal(EntityUid uid)
    {
        var ev = new MCXenoRageDeactivateEvent();
        RaiseLocalEvent(uid, ref ev);
    }
}
