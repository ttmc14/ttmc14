using Content.Shared._MC.Xeno.Biomass;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Psydrain;

public sealed class MCXenoPsydrainSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedRMCFlammableSystem _flammable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;

    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = default!;

    [Dependency] private readonly MCStatusSystem _mcStatus = default!;
    [Dependency] private readonly MCXenoBiomassSystem _mcXenoBiomass = default!;

    private EntityQuery<MCXenoPsydrainableComponent> _psydrainableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _psydrainableQuery = GetEntityQuery<MCXenoPsydrainableComponent>();

        SubscribeLocalEvent<MCXenoPsydrainComponent, MCXenoPsydrainActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPsydrainComponent, MCXenoPsydrainDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<MCXenoPsydrainableComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoPsydrainableComponent, MobStateChangedEvent>(OnPsydrainableStateChanged);
    }

    private void OnStartup(Entity<MCXenoPsydrainableComponent> entity, ref ComponentStartup args)
    {
        // entity.Comp.Available = !_mobState.IsDead(entity.Owner);
        // Dirty(entity);
    }

    private void OnAction(Entity<MCXenoPsydrainComponent> entity, ref MCXenoPsydrainActionEvent args)
    {
        var target = args.Target;

        if (args.Handled)
            return;

        if (!_psydrainableQuery.TryComp(target, out var psydrainableComponent))
        {
            _popup.PopupClient(Loc.GetString("psydrain-not-human"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!psydrainableComponent.Available)
        {
            _popup.PopupClient(Loc.GetString("someone-already-psydrained"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            var notDead = Loc.GetString("psydrain-not-dead");
            _popup.PopupClient(notDead, entity, entity, PopupType.MediumXeno);
            return;
        }

        if (_flammable.IsOnFire(entity.Owner))
        {
            _popup.PopupClient(Loc.GetString("psydrain-our-fire"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_rmcXenoHive.HasHive(entity.Owner))
        {
            _popup.PopupClient(Loc.GetString("psydrain-dont-have-hive"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_rmcActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        args.Handled = true;

        _popup.PopupClient(Loc.GetString("being-psydrained", ("entity", entity), ("target", target)), entity, entity, PopupType.MediumXeno);
        _audio.PlayPredicted(entity.Comp.SoundDrain, entity, entity);

        var ev = new MCXenoPsydrainDoAfterEvent(args.Action, EntityManager);
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BlockDuplicate = true,
            CancelDuplicate = true,
            RequireCanInteract = true,
            BreakOnRest = true,
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            return;

        _popup.PopupClient(Loc.GetString("doAfter-canceled-owner"), entity, entity, PopupType.MediumXeno);

       // TODO: Fix this
        //if (sound.HasValue)
            //_audio.Stop(entity);
    }

    private void OnDoAfter(Entity<MCXenoPsydrainComponent> entity, ref MCXenoPsydrainDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            _popup.PopupClient(Loc.GetString("doAfter-canceled-owner"), entity, entity, PopupType.MediumXeno);
            return;
        }

        var actionUid = args.GetActionUid(EntityManager);

        if (args.Target is not { } target)
            return;

        if (!_psydrainableQuery.TryComp(target, out var psydrainableComponent))
            return;

        if (!psydrainableComponent.Available)
        {
            _popup.PopupClient(Loc.GetString("someone-already-psydrained"), entity, entity, PopupType.MediumXeno);
            return;
        }

        if (!_rmcActions.TryUseAction(entity, actionUid, target))
            return;

        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoPsydrainActionEvent>(entity))
        {
            if (action.Owner != actionUid)
                continue;

            _actions.StartUseDelay((action, action));
            break;
        }

        psydrainableComponent.Available = false;
        Dirty(target, psydrainableComponent);

        args.Handled = true;

        // Effects
        _audio.PlayPredicted(entity.Comp.SoundDrainEnd, entity, entity);
        _popup.PopupClient(Loc.GetString("end-drain-owner", ("target", target)), entity, entity, PopupType.MediumXeno);
        _jittering.DoJitter(entity, entity.Comp.JitteringDelayOwner, true, entity.Comp.JitteringAmplitudeOwner, entity.Comp.JitteringFrequencyOwner);
        _jittering.DoJitter(target, entity.Comp.JitteringDelayTarget, true, entity.Comp.JitteringAmplitudeTarget, entity.Comp.JitteringFrequencyTarget);

        // Damage
        _damageable.TryChangeDamage(target, entity.Comp.Damage);

        // Biomass
        _mcXenoBiomass.Add(entity.Owner, entity.Comp.BiomassGain);

        // Hive reward
        _rmcXenoHive.AddLarvaPointsOwner(entity, entity.Comp.LarvaPointsGain);

        var psypointReward = int.Clamp(
            entity.Comp.PsypointRewardMin + (MCStatusSystem.HighPlayerPop - _mcStatus.ActivePlayerCount) / MCStatusSystem.HighPlayerPop * (entity.Comp.PsypointRewardMax - entity.Comp.PsypointRewardMin),
            entity.Comp.PsypointRewardMin,
            entity.Comp.PsypointRewardMax);

        _rmcXenoHive.AddPsypointsFromOwner(entity, "Strategic", psypointReward);
        _rmcXenoHive.AddPsypointsFromOwner(entity, "Tactical", psypointReward / 4);

        _adminLogger.Add(LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(entity.Owner):player} successfully used Psy Drain on {ToPrettyString(target):target} " +
            $"at {Transform(target).Coordinates:coordinates}. " +
            $"Larva points gained: {entity.Comp.LarvaPointsGain}, " +
            $"Biomass points gained: {entity.Comp.BiomassGain}, " +
            $"Psy points gained: {psypointReward}, " +
            $"Damage applied: {entity.Comp.Damage}");
    }

    private void OnPsydrainableStateChanged(Entity<MCXenoPsydrainableComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.OldMobState != MobState.Dead || args.NewMobState == MobState.Dead)
            return;

        ent.Comp.Available = true;
        Dirty(ent);
    }
}
