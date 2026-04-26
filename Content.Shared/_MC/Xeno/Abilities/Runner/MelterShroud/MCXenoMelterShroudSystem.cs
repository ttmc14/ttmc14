using Content.Shared._MC.Xeno.Abilities.Runner.MelterShroud.Events.Action;
using Content.Shared._RMC14.Xenonids.Hive;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Runner.MelterShroud;

public sealed class MCXenoMelterShroudSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoMelterShroudComponent, MCXenoMelterShroudActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoMelterShroudComponent> entity, ref MCXenoMelterShroudActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        var smokeUid = ServerSpawn(entity.Comp.ShroudId, _transform.GetMapCoordinates(entity));
        if (!smokeUid.Valid)
            return;

        MCXenoHive.SetSameHive(entity.Owner, smokeUid);

        _audio.PlayPvs(entity.Comp.EffectSound, Transform(smokeUid).Coordinates);
    }
}
