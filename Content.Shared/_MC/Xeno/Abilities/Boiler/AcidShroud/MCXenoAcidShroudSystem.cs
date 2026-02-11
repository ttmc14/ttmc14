using Content.Shared._MC.Popup;
using Content.Shared._MC.Spreader;
using Content.Shared._MC.Xeno.Abilities.Boiler.AcidShroud.Events.Action;
using Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.AcidShroud;

public sealed class MCXenoAcidShroudSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly MCXenoGlobSystem _glob = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoAcidShroudComponent, MCXenoAcidShroudActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoAcidShroudComponent> entity, ref MCXenoAcidShroudActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_glob.TryGetShroudId(entity.Owner, out var shroudId))
        {
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-bombard-launch-cancelled-no-projectile", PopupType.MediumCaution);
            return;
        }

        if (!TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        var smokeUid = ServerSpawn(shroudId, _transform.GetMapCoordinates(entity));
        if (!smokeUid.Valid)
            return;

        _rmcXenoHive.SetSameHive(entity.Owner, smokeUid);
        _audio.PlayPvs(entity.Comp.EffectSound, Transform(smokeUid).Coordinates);

        var spreader = EnsureComp<MCEdgeSpreaderComponent>(smokeUid);
        spreader.Range = int.Max(_glob.GetGlobCount(entity.Owner), entity.Comp.MinRange);
        Dirty(smokeUid, spreader);

        _glob.SetGlobCount(entity.Owner, 0, popup: true);
    }
}
