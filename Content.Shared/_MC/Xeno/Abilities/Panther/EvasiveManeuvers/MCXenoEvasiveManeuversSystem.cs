using Content.Shared._MC.Mob.Movement;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Panther.EvasiveManeuvers;

public sealed partial class MCXenoEvasiveManeuversSystem : MCXenoAbilitySystem
{
    private static readonly LocId BeginLocId = "mc-xeno-evasion-maneuvers-begin";
    private static readonly LocId EndLocId = "mc-xeno-evasion-maneuvers-end";
    private static readonly LocId NoPlasmaLocId = "mc-xeno-evasion-maneuvers-no-plasma";
    private static readonly LocId InterruptedLocId = "mc-xeno-evasion-maneuvers-interrupted";
    private static readonly LocId NoMovementLocId = "mc-xeno-evasion-maneuvers-no-movement";

    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedJitteringSystem _jittering = null!;

    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    private EntityQuery<ProjectileComponent> _projectileQuery;

    public override void Initialize()
    {
        _projectileQuery = GetEntityQuery<ProjectileComponent>();

        InitializeDebuff();

        SubscribeLocalEvent<MCXenoEvasiveManeuversComponent, MCMobStepEvent>(OnStep);
        SubscribeLocalEvent<MCXenoEvasiveManeuversComponent, MCXenoEvasiveManeuversActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoEvasiveManeuversComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnStep(Entity<MCXenoEvasiveManeuversComponent> entity, ref MCMobStepEvent args)
    {
        if (!entity.Comp.Active)
            return;

        entity.Comp.LastMove = _timing.CurTime;
        Dirty(entity);
    }

    private void OnAction(Entity<MCXenoEvasiveManeuversComponent> entity, ref MCXenoEvasiveManeuversActionEvent args)
    {
        if (entity.Comp.Active)
        {
            Deactivate(entity);
            return;
        }

        Activate(entity);
    }

    private void OnPreventCollide(Entity<MCXenoEvasiveManeuversComponent> entity, ref PreventCollideEvent args)
    {
        if (!entity.Comp.Active)
            return;

        if (args.Cancelled || !_projectileQuery.HasComp(args.OtherEntity))
            return;

        args.Cancelled = true;

        _audio.PlayPredicted(entity.Comp.EffectSoundEvasion, entity, entity);
        _jittering.DoJitter(entity, TimeSpan.FromSeconds(0.5), true, frequency: 6);
    }

    private void Activate(Entity<MCXenoEvasiveManeuversComponent> entity)
    {
        _popup.PopupClient(Loc.GetString(BeginLocId), entity, entity, PopupType.Medium);

        entity.Comp.Active = true;
        entity.Comp.LastMove = _timing.CurTime;
        Dirty(entity);

        ActionSetToggled<MCXenoEvasiveManeuversActionEvent>(entity, true);
    }

    private void Deactivate(Entity<MCXenoEvasiveManeuversComponent> entity)
    {
        if (!entity.Comp.Active)
            return;

        _popup.PopupClient(Loc.GetString(EndLocId), entity, entity, PopupType.Medium);

        entity.Comp.Active = false;
        Dirty(entity);

        ActionSetToggled<MCXenoEvasiveManeuversActionEvent>(entity, false);
        ActionStartUseDelay<MCXenoEvasiveManeuversActionEvent>(entity);
    }
}
