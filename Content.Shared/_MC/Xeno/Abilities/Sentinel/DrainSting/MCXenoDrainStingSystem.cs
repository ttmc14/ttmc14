using Content.Shared._MC.Damage;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Abilities.Sentinel.ToxicStacks;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Popups;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.DrainSting;

public sealed class MCXenoDrainStingSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly XenoPlasmaSystem _rmcXenoPlasma = null!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = null!;

    [Dependency] private readonly MCXenoToxicStacksSystem _mcXenoToxicStacks = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;
    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoDrainStingComponent, MCXenoDrainStingActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoDrainStingComponent> entity, ref MCXenoDrainStingActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MCXenoToxicStacksComponent>(args.Target, out var toxicStacksComponent))
        {
            _popup.PopupClient("Immune to intoxication", entity, entity);
            return;
        }

        if (toxicStacksComponent.Count == 0)
        {
            _popup.PopupClient("Not intoxicated", entity, entity);
            return;
        }

        if (!RMCActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        var stacks = toxicStacksComponent.Count;
        var drainPotency = stacks * entity.Comp.PotencyMultiplier;

        if (stacks > toxicStacksComponent.Max - 10)
            _rmcEmote.TryEmoteWithChat(args.Target, "Scream");

        // TODO: bonus armor

        var damage = entity.Comp.Damage * drainPotency / 5;
        var paralyzeDuration = TimeSpan.FromSeconds(Math.Max(0.1f, (stacks - 10f) / 10f));

        _mcDamageable.AdjustBurnLoss(args.Target, damage);
        _mcStun.Paralyze(args.Target, paralyzeDuration);

        _mcXenoHeal.Heal(entity, drainPotency);
        _rmcXenoPlasma.RegenPlasma(entity.Owner, drainPotency * 3.5f);
        _mcXenoToxicStacks.TryAdd(args.Target, (int) -float.Round(stacks * 0.7f));

        AnimateHit(entity, args.Target);
    }
}
