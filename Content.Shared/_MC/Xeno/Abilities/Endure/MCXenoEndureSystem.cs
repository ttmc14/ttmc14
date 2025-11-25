using Content.Shared._MC.Armor;
using Content.Shared._MC.Stun.Events;
using Content.Shared._RMC14.Aura;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Xenonids.Pheromones;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Endure;

public sealed class MCXenoEndureSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedAuraSystem _rmcAura = null!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoEndureComponent, MCXenoEndureActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoEndureActiveComponent, ComponentRemove>(OnActiveRemove);
        SubscribeLocalEvent<MCXenoEndureActiveComponent, MCStunAttemptEvent>(OnActiveTryStun);
        SubscribeLocalEvent<MCXenoEndureActiveComponent, MCArmorGetEvent>(OnActiveArmorGet);
        SubscribeLocalEvent<MCXenoEndureActiveComponent, UpdateMobStateEvent>(OnActiveUpdateMobState,
            after: [typeof(MobThresholdSystem), typeof(SharedXenoPheromonesSystem)]);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoEndureActiveComponent>();
        while (query.MoveNext(out var uid, out var activeComponent))
        {
            if (_timing.CurTime < activeComponent.EndTime)
            {
                var second = (int) double.Round((activeComponent.EndTime - _timing.CurTime).TotalSeconds);
                if (second == activeComponent.LastShowedTime)
                    continue;

                activeComponent.LastShowedTime = second;

                if (_net.IsServer)
                    _popup.PopupEntity(second.ToString(), uid, uid, PopupType.MediumXeno);

                continue;
            }

            RemCompDeferred<MCXenoEndureActiveComponent>(uid);
        }
    }

    private void OnAction(Entity<MCXenoEndureComponent> entity, ref MCXenoEndureActionEvent args)
    {
        if (args.Handled)
            return;

        if (!RMCActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (!HasComp<AuraComponent>(entity))
            _rmcAura.GiveAura(entity, entity.Comp.ActivationAuraColor, entity.Comp.Duration);

        _rmcEmote.TryEmoteWithChat(entity, entity.Comp.ActivationEmote);

        var activeComponent = EnsureComp<MCXenoEndureActiveComponent>(entity);
        activeComponent.EndTime = _timing.CurTime + entity.Comp.Duration;
    }

    private void OnActiveTryStun(Entity<MCXenoEndureActiveComponent> ent, ref MCStunAttemptEvent args)
    {
        args.Canceled = true;
    }

    private void OnActiveRemove(Entity<MCXenoEndureActiveComponent> entity, ref ComponentRemove args)
    {
        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("mc-xeno-ability-endure-end"), entity, entity, PopupType.MediumXeno);

        _mobState.UpdateMobState(entity);
    }

    private void OnActiveArmorGet(Entity<MCXenoEndureActiveComponent> entity, ref MCArmorGetEvent args)
    {
        args.ArmorDefinition.Bomb += 20;
        args.ArmorDefinition.Melee += 40;
        args.ArmorDefinition.Fire += 30;
    }

    private void OnActiveUpdateMobState(Entity<MCXenoEndureActiveComponent> entity, ref UpdateMobStateEvent args)
    {
        if (args.Component.CurrentState == MobState.Dead || args.State == MobState.Dead)
            return;

        args.State = MobState.Alive;
    }
}
