using Content.Shared._MC.Damage.Extensions;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Emote;
using Content.Shared.Damage;

namespace Content.Shared._MC.Xeno.Abilities.Defender.RegenerateSkin;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoRegenerateSkinSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly MCXenoSunderSystem _mcXenoSunder = null!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmoteSystem = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRegenerateSkinComponent, MCXenoRegenerateSkinActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoRegenerateSkinComponent> entity, ref MCXenoRegenerateSkinActionEvent args)
    {
        if (args.Handled || !TryUseAction(entity, args.Action, allowUseOnFire: false))
            return;

        args.Handled = true;

        _mcXenoSunder.SetSunder(entity.Owner, entity.Comp.HealSunder);
        _damageable.TryHealDamageExt(entity, entity.Comp.HealDamage);

        _rmcEmoteSystem.TryEmoteWithChat(entity, entity.Comp.EffectEmote);
        RaiseEffect(entity, entity.Comp.EffectColor);
    }
}
