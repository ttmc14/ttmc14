using System.Numerics;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Abilities;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._MC.Xeno.Plasma.Components.Conversions;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Construction.Nest;
using Content.Shared._RMC14.Xenonids.Devour;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Marines;
using Content.Shared.Stunnable;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Jittering;
using Content.Shared.Examine;
using Content.Shared.Coordinates;
using Robust.Shared.GameObjects;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer.DreadfulPresence;

public sealed class MCXenoDreadfulPresenceSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = default!;
    [Dependency] private readonly MCStunSystem _mcStun = default!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;

    private readonly HashSet<Entity<MobStateComponent>> _mobs = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoDreadfulPresenceComponent, MCXenoDreadfulPresenceActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoDreadfulPresenceComponent> ent, ref MCXenoDreadfulPresenceActionEvent args)
    {
        if (args.Handled)
            return;

        if (!RMCActions.TryUseAction(ent, args.Action, ent))
            return;

        args.Handled = true;

        _lookup.GetEntitiesInRange(ent.Owner.ToCoordinates(), ent.Comp.DreadRange, _mobs);

        foreach (var receiver in _mobs)
        {
            if (!IsAffectableEntity(ent.Owner, receiver))
                continue;

            _rmcEmote.TryEmoteWithChat(receiver, ent.Comp.HumanEmote, forceEmote: true);

            _stun.TrySlowdown(receiver, ent.Comp.DreadTime, false,
                ent.Comp.WalkSpeedModifier, 
                ent.Comp.SprintSpeedModifier);

            _jitter.DoJitter(
                receiver,
                TimeSpan.FromSeconds(3),
                refresh: true,
                frequency: 8f,
                forceValueChange: true
            );

            _popup.PopupClient(ent.Comp.Popup, receiver);
        }
    }

    private bool IsAffectableEntity(EntityUid user, EntityUid target)
    {
        if (user == target)
            return false;

        if (_hive.FromSameHive(user, target))
            return false;

        if (_mobState.IsDead(target))
            return false;

        if (HasComp<DevouredComponent>(target))
            return false;

        if (HasComp<XenoNestedComponent>(target))
            return false;

        if (!_examine.InRangeUnOccluded(user, target))
            return false;

        return HasComp<MarineComponent>(target);
    }
}
