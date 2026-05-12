using Content.Shared.Popups;

namespace Content.Shared._MC.Xeno.Abilities.Panther.EvasiveManeuvers;

public sealed partial class MCXenoEvasiveManeuversSystem
{
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCXenoEvasiveManeuversComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Active)
                continue;

            if (component.UpdateNext > _timing.CurTime)
                continue;

            component.UpdateNext = _timing.CurTime + component.UpdateDelay;
            Dirty(uid, component);

            DoUpdate((uid, component));
        }
    }

    private void DoUpdate(Entity<MCXenoEvasiveManeuversComponent> entity)
    {
        var recentlyMoved = entity.Comp.LastMove + entity.Comp.MoveTolerance >= _timing.CurTime;
        if (!recentlyMoved)
        {
            _popup.PopupClient(Loc.GetString(NoMovementLocId), entity, entity, PopupType.MediumCaution);
            Deactivate(entity);
            return;
        }

        var modifier = (float) entity.Comp.UpdateDelay.TotalSeconds;
        var drain = entity.Comp.PlasmaDrain * modifier;

        if (!_mcXenoPlasma.HasPlasma(entity, drain))
        {
            _popup.PopupClient(Loc.GetString(NoPlasmaLocId), entity, entity, PopupType.MediumCaution);
            Deactivate(entity);
            return;
        }

        _mcXenoPlasma.RemovePlasma(entity, drain);
    }
}
