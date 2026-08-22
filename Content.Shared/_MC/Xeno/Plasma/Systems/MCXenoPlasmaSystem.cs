using Content.Shared._MC.Xeno.Plasma.Components;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Xeno.Plasma.Systems;

public sealed class MCXenoPlasmaSystem : EntitySystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = null!;

    private EntityQuery<XenoPlasmaComponent> _query;
    private EntityQuery<MCXenoPlasmaComponent> _mcQuery;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<XenoPlasmaComponent>();
        _mcQuery = GetEntityQuery<MCXenoPlasmaComponent>();
    }

    public bool TryTransferPlasma(EntityUid sourceUid, EntityUid targetUid, float amount)
    {
        if (!CanTransferPlasma(sourceUid, targetUid, amount))
            return false;

        RemovePlasma(sourceUid, amount);
        RegenPlasma(targetUid, amount);
        return true;
    }

    public bool CanTransferPlasma(EntityUid sourceUid, EntityUid targetUid, float amount)
    {
        if (!_query.TryComp(sourceUid, out _))
            return false;

        if (!_query.TryComp(targetUid, out _) || !_mcQuery.TryComp(targetUid, out var mcTargetComponent))
            return false;

        if (!mcTargetComponent.CanBeGivenPlasma)
            return false;

        return !IsFullPlasma(targetUid) && HasPlasma(sourceUid, amount);
    }

    public bool HasPlasma(EntityUid uid, float amount)
    {
        return _query.TryComp(uid, out var component)
               && _xenoPlasma.HasPlasma((uid, component), amount);
    }

    #region Regen

    public void RegenPlasma(EntityUid uid, float amount)
    {
        if (!_query.TryComp(uid, out var component))
            return;

        _xenoPlasma.RegenPlasma((uid, component), amount);
    }

    public void RegenPlasma(Entity<XenoPlasmaComponent?> entity, float amount)
    {
        if (!_query.Resolve(entity, ref entity.Comp, false))
            return;

        _xenoPlasma.RegenPlasma(entity, amount);
    }

    public void RegenPlasmaMax(EntityUid uid)
    {
        if (!_query.TryComp(uid, out var component))
            return;

        _xenoPlasma.RegenPlasma((uid, component), component.MaxPlasma);
    }

    #endregion

    #region Remove

    public void RemovePlasma(EntityUid uid, float amount)
    {
        if (!_query.TryComp(uid, out var component))
            return;

        _xenoPlasma.RemovePlasma((uid, component), amount);
    }

    public void RemovePlasma(Entity<XenoPlasmaComponent?> entity, float amount)
    {
        if (!_query.Resolve(entity, ref entity.Comp, false))
            return;

        _xenoPlasma.RemovePlasma((entity, entity.Comp), amount);
    }

    public bool TryRemovePlasma(EntityUid uid, float amount)
    {
        if (!_query.TryComp(uid, out var component))
            return false;

        var previous = component.Plasma;
        if (previous == FixedPoint2.Zero)
            return false;

        RemovePlasma((uid, component), amount);
        return previous >= amount;
    }

    #endregion

    #region Get

    public float GetPlasma(EntityUid uid)
    {
        if (!_query.TryComp(uid, out var component))
            return 0;

        var plasma = component.Plasma;
        return plasma.Float();
    }

    public float GetMaxPlasma(EntityUid uid)
    {
        return !_query.TryComp(uid, out var component)
            ? 0
            : component.MaxPlasma;
    }

    public float GetPlasmaNormalized(EntityUid uid)
    {
        if (!_query.TryComp(uid, out var component))
            return 0;

        if (component.MaxPlasma == 0)
            return 0;

        var plasma = component.Plasma;
        return float.Clamp(plasma.Float() / component.MaxPlasma, 0, 1);
    }

    #endregion

    public bool IsFullPlasma(EntityUid uid)
    {
        if (!_query.TryComp(uid, out var component))
            return false;

        var plasmaFixed = component.Plasma;
        var plasma = plasmaFixed.Float();
        var plasmaMax = component.MaxPlasma;

        return plasmaMax - plasma <= -1e4;
    }
}
