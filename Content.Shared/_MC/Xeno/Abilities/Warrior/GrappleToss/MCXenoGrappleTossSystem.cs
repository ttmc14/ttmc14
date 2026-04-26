using Content.Shared._MC.Knockback;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Abilities.Warrior.Agility;
using Content.Shared.Movement.Pulling.Components;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.GrappleToss;

public sealed class MCXenoGrappleTossSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;
    [Dependency] private readonly MCXenoAgilitySystem _mcXenoAgility = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoGrappleTossComponent, MCXenoGrappleTossActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoGrappleTossComponent> entity, ref MCXenoGrappleTossActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PullerComponent>(entity, out var pullerComponent) || pullerComponent.Pulling is not { } targetEntity)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        _mcXenoAgility.Disable(entity);

        if (!MCXenoHive.FromSameHive(entity.Owner, targetEntity))
        {
            _mcStun.Slowdown(targetEntity, entity.Comp.SlowdownDuration);
            _mcStun.Paralyze(targetEntity, entity.Comp.ParalyzeDuration);
        }

        var origin = _transform.GetMapCoordinates(entity);
        var direction = args.Target.Position - origin.Position;

        _mcKnockback.Knockback(targetEntity, direction, entity.Comp.KnockbackEntry);
    }
}
