using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Flay;

public sealed class MCXenoFlaySystem : EntitySystem
{
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = default!;
    [Dependency] private readonly RMCSizeStunSystem _rmcStun = default!;
    [Dependency] private readonly XenoEnergySystem _xenoEnergy = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoFlayComponent, MCXenoFlayActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoFlayComponent> ent, ref MCXenoFlayActionEvent args)
    {
        if (args.Handled)
            return

        if (_mobState.IsDead(args.Target))
            return;

        if (_xenoHive.FromSameHive(ent.Owner, args.Target))
            return;

        if (!_rmcActions.TryUseAction(ent, args.Action, ent))
            return;

        args.Handled = true;

        _damageable.TryChangeDamage(args.Target, ent.Comp.Damage, origin: ent, tool: ent);
        _rmcStun.TryKnockOut(args.Target, ent.Comp.ParalyzeTime);
        _xenoEnergy.AddEnergy(ent.Owner, ent.Comp.GainEnergy);

        _rmcEmote.TryEmoteWithChat(args.Target, ent.Comp.HumanEmote, forceEmote: true);
        _rmcEmote.TryEmoteWithChat(ent.Owner, ent.Comp.XenoEmote);

        _popup.PopupEntity(Loc.GetString(ent.Comp.Popup, ("xeno", ent.Owner), ("target", args.Target)));
    }
}
