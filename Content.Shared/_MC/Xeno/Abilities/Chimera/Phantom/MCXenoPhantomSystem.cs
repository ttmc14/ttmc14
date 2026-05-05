using Content.Shared._RMC14.Xenonids.Invisibility;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Chimera.Phantom;

public sealed class MCXenoPhantomSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoPhantomComponent, MCXenoPhantomActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPhantomComponent, PreventCollideEvent>(OnPreventCollision);
    }

    private void OnAction(Entity<MCXenoPhantomComponent> entity, ref MCXenoPhantomActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        ApplyInvisible(entity);

        var coordinates = Transform(entity).Coordinates;
        var instance = PredictedSpawnAtPosition(entity.Comp.EntProtoId, coordinates);

        EnsureComp<MCXenoPhantomCloneComponent>(instance);

        MCXenoHive.SetSameHive(entity.Owner, instance);

        _audio.PlayPredicted(entity.Comp.EffectSound, entity, entity);

        args.Handled = true;
    }

    private void OnPreventCollision(Entity<MCXenoPhantomComponent> ent, ref PreventCollideEvent args)
    {
        if (!HasComp<MCXenoPhantomCloneComponent>(args.OtherEntity))
            return;

        args.Cancelled = true;
    }

    private void ApplyInvisible(Entity<MCXenoPhantomComponent> entity)
    {
        var invisible = EnsureComp<XenoActiveInvisibleComponent>(entity);

        invisible.ExpiresAt = _timing.CurTime + entity.Comp.InvisibleDuration;
        invisible.FullCooldown = TimeSpan.Zero;
        invisible.SpeedMultiplier = FixedPoint2.New(1);
        invisible.Opacity = 0.05f;

        Dirty(entity, invisible);
    }
}
