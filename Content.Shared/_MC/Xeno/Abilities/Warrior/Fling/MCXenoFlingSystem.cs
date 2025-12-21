using Content.Shared._MC.CameraShake;
using Content.Shared._MC.Knockback;
using Content.Shared._MC.Xeno.Abilities.Warrior.Agility;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.Fling;

public sealed class MCXenoFlingSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;

    [Dependency] private readonly MCCameraShakeSystem _mcCameraShake = null!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;
    [Dependency] private readonly MCXenoAgilitySystem _mcXenoAgility = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoFlingComponent, MCXenoFlingActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoFlingComponent> entity, ref MCXenoFlingActionEvent args)
    {
        if (args.Handled || !TryUseAction(entity, args.Action, args.Target))
            return;

        args.Handled = true;

        _mcXenoAgility.Disable(entity);

        var knockback = IsBig(args.Target) ? entity.Comp.KnockbackBigEntry : entity.Comp.KnockbackEntry;
        _mcKnockback.KnockbackFrom(args.Target, entity, knockback);

        _audio.PlayPredicted(entity.Comp.EffectsSound, entity, entity);
        _mcCameraShake.ShakeCamera(args.Target, entity.Comp.CameraShakeEntry);

        AnimateHit(entity, args.Target);

        // if (empowered)
        //    knockback.Distance *= entity.Comp.EmpowerMultiplier;
    }
}
