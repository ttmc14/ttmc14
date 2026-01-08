using Content.Shared._MC.Armor;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Popup;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Abilities.Sentinel.ToxicStacks;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.DrainSting;

public sealed class MCXenoDrainStingSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
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

        SubscribeLocalEvent<MCXenoDrainStingBuffComponent, ComponentStartup>(OnBuffStartup);
        SubscribeLocalEvent<MCXenoDrainStingBuffComponent, ComponentRemove>(OnBuffRemove);
        SubscribeLocalEvent<MCXenoDrainStingBuffComponent, MCArmorGetEvent>(OnBuffArmorGet);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoDrainStingBuffComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.EndTime)
                continue;

            RemCompDeferred<MCXenoDrainStingBuffComponent>(uid);
        }
    }

    private void OnAction(Entity<MCXenoDrainStingComponent> entity, ref MCXenoDrainStingActionEvent args)
    {
        if (args.Handled)
            return;

        if (_mcXenoToxicStacks.HasImmune(args.Target))
        {
            _popup.PopupClient(Loc.GetString("mc-xeno-ability-drain-sting-immune"), entity, entity, PopupType.SmallCaution);
            return;
        }

        var stacks = _mcXenoToxicStacks.Get(args.Target);
        var stacksMax = _mcXenoToxicStacks.GetMax(args.Target);

        if (stacks == 0)
        {
            _popup.PopupClient(Loc.GetString("mc-xeno-ability-drain-sting-not-intoxicated"), entity, entity, PopupType.SmallCaution);
            return;
        }

        if (!TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        ServerSpawnAttachedTo(entity.Comp.EffectId, args.Target);

        if (stacks > stacksMax - entity.Comp.BuffStackMargin)
        {
            _rmcEmote.TryEmoteWithChat(args.Target, entity.Comp.BuffTargetEmote);
            ApplyBuff(entity);
        }

        var drainPotency = stacks * entity.Comp.PotencyMultiplier;

        _mcStun.Paralyze(args.Target, GetParalyzeDuration(entity, stacks));
        _mcDamageable.AdjustBurnLoss(args.Target, drainPotency * entity.Comp.BurnDamageMultiplier);

        _mcXenoHeal.Heal(entity, drainPotency * entity.Comp.PlasmaRegenMultiplier);
        _rmcXenoPlasma.RegenPlasma(entity.Owner, drainPotency * entity.Comp.PlasmaRegenMultiplier);

        _mcXenoToxicStacks.Remove(args.Target, -stacks * entity.Comp.ToxicDrainRatio);

        AnimateHit(entity, args.Target);
    }

    private void ApplyBuff(Entity<MCXenoDrainStingComponent> entity)
    {
        var component = EnsureComp<MCXenoDrainStingBuffComponent>(entity);
        component.EndTime = _timing.CurTime + entity.Comp.BuffDuration;

        DirtyField(entity, component, nameof(MCXenoDrainStingBuffComponent.EndTime));
    }

    private void OnBuffStartup(Entity<MCXenoDrainStingBuffComponent> entity, ref ComponentStartup args)
    {
        _popup.PopupEntityServer(Loc.GetString("mc-xeno-ability-drain-sting-buff-start"), entity, PopupType.MediumXeno);
    }

    private void OnBuffRemove(Entity<MCXenoDrainStingBuffComponent> entity, ref ComponentRemove args)
    {
        _popup.PopupEntityServer(Loc.GetString("mc-xeno-ability-drain-sting-buff-end"), entity, PopupType.MediumXeno);
    }

    private static void OnBuffArmorGet(Entity<MCXenoDrainStingBuffComponent> entity, ref MCArmorGetEvent args)
    {
        args.ArmorDefinition += entity.Comp.Armor;
    }

    private static TimeSpan GetParalyzeDuration(Entity<MCXenoDrainStingComponent> entity, int stacks)
    {
        return TimeSpan.FromSeconds(
            Math.Max(
                entity.Comp.ParalyzeMinSeconds,
                (stacks - entity.Comp.ParalyzeThreshold) * entity.Comp.ParalyzeMultiplier
            )
        );
    }
}
