using Content.Shared._MC.Aura;
using Content.Shared._MC.Xeno.Abilities.Gorger.Feast.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Feast.Events;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Feast;

public sealed class MCXenoFeastSystem : MCXenoAbilitySystem
{
    private const string AuraLayer = "feast";

    [Dependency] private readonly MCAuraSystem _aura = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly MCXenoOverhealSystem _overheal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCXenoFeastActiveComponent, ComponentShutdown>(OnAuraShutdown);

        SubscribeLocalEvent<MCXenoFeastComponent, MCXenoFeastActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoFeastActiveComponent, MobStateChangedEvent>(OnState);
    }

    private void OnAction(Entity<MCXenoFeastComponent> ent, ref MCXenoFeastActionEvent args)
    {
        if (args.Handled || !CanUseAction(ent, args.Action))
            return;

        if (TryComp<MCXenoFeastActiveComponent>(ent, out var active))
        {
            Stop((ent, active));
            args.Handled = true;
            return;
        }

        if (!_plasma.HasPlasma(ent, ent.Comp.MinimumBlood))
            return;

        args.Handled = true;

        active = EnsureComp<MCXenoFeastActiveComponent>(ent);
        active.NextTick = _timing.CurTime + ent.Comp.Interval;
        active.ReadyAt = _timing.CurTime + ent.Comp.Cooldown;
        Dirty(ent.Owner, active);

        _audio.PlayPredicted(ent.Comp.StartSound, ent, ent);
        _aura.Set(ent, AuraLayer, ent.Comp.Aura);

        ActionSetToggled<MCXenoFeastActionEvent>(ent, true);
        ActionSetUseDelay<MCXenoFeastActionEvent>(ent, ent.Comp.CancelDelay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (Net.IsClient)
            return;

        var query = EntityQueryEnumerator<MCXenoFeastActiveComponent, MCXenoFeastComponent>();
        while (query.MoveNext(out var uid, out var active, out var comp))
        {
            if (active.NextTick > _timing.CurTime)
                continue;

            if (!_plasma.HasPlasma(uid, comp.Cost))
            {
                Stop((uid, active));
                continue;
            }

            active.NextTick = _timing.CurTime + comp.Interval;
            Dirty(uid, active);

            _plasma.RemovePlasma(uid, comp.Cost);

            var amount = _heal.GetHealthAlive(uid) * comp.HealFraction;

            _overheal.Restore(uid, amount, 0.5f);
            _overheal.RestoreNearby(uid, comp.Range, amount, 0.5f, "MCEffectBloodHealing");
        }
    }

    private void OnAuraShutdown(Entity<MCXenoFeastActiveComponent> ent, ref ComponentShutdown args) =>
        _aura.Remove(ent, AuraLayer);

    private void OnState(Entity<MCXenoFeastActiveComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            Stop(ent);
    }

    private void Stop(Entity<MCXenoFeastActiveComponent> ent)
    {
        var delay = ent.Comp.ReadyAt > _timing.CurTime ? ent.Comp.ReadyAt - _timing.CurTime : TimeSpan.Zero;

        RemComp<MCXenoFeastActiveComponent>(ent);
        ActionSetToggled<MCXenoFeastActionEvent>(ent, false);
        ActionSetUseDelay<MCXenoFeastActionEvent>(ent, delay);
        ActionSetCooldown<MCXenoFeastActionEvent>(ent, delay);
    }
}
