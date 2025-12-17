using Content.Shared._MC.Flammable;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Emote;
using Content.Shared.Damage;

namespace Content.Shared._MC.Xeno.Abilities.RegenerateSkin;

public sealed class MCXenoRegenerateSkinSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MCSharedFlammableSystem _mcFlammable = default!;
    [Dependency] private readonly MCXenoSunderSystem _mcXenoSunder = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmoteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRegenerateSkinComponent, MCXenoRegenerateSkinActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoRegenerateSkinComponent> entity, ref MCXenoRegenerateSkinActionEvent args)
    {
        if (args.Handled)
            return;

        if (_mcFlammable.OnFire(entity))
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        _rmcEmoteSystem.TryEmoteWithChat(entity, entity.Comp.Emote);
        _mcXenoSunder.SetSunder(entity.Owner, entity.Comp.Sunder);
        _damageable.TryChangeDamage(entity, -entity.Comp.Heal, true, false);
    }
}
