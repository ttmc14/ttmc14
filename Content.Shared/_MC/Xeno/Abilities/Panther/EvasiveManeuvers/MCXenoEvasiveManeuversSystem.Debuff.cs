using Content.Shared._MC.Stun.Events;
using Content.Shared.Popups;
using Content.Shared.Projectiles;

namespace Content.Shared._MC.Xeno.Abilities.Panther.EvasiveManeuvers;

public sealed partial class MCXenoEvasiveManeuversSystem
{
    private void InitializeDebuff()
    {
        _projectileQuery = GetEntityQuery<ProjectileComponent>();

        SubscribeLocalEvent<MCXenoEvasiveManeuversComponent, MCStunAttemptEvent>(OnStunAttempt);
    }

    private void OnStunAttempt(Entity<MCXenoEvasiveManeuversComponent> entity, ref MCStunAttemptEvent args)
    {
        if (!DoDebuff(entity))
            return;

        args.Canceled = true;
    }

    private bool DoDebuff(Entity<MCXenoEvasiveManeuversComponent> entity)
    {
        if (entity.Comp.Active)
            return false;

        _popup.PopupClient(Loc.GetString(InterruptedLocId), entity, entity, PopupType.MediumCaution);

        if (!_mcXenoPlasma.HasPlasma(entity, entity.Comp.PlasmaInterruptDrain))
        {
            Deactivate(entity);
            return false;
        }

        _mcXenoPlasma.RemovePlasma(entity, entity.Comp.PlasmaInterruptDrain);
        return true;
    }
}
