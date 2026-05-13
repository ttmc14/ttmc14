using Content.Shared._MC.Engineering.Deploy;
using Content.Shared._MC.Engineering.Linking.Pair;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Engineering.Teleport;

public sealed class MCTeleporterSystem : EntitySystem
{
    private static readonly LocId NoLinkLocId = "mc-teleporter-no-link";
    private static readonly LocId InvalidLinkLocId = "mc-teleporter-invalid-link";
    private static readonly LocId RechargingLocId = "mc-teleporter-recharging";
    private static readonly LocId NothingToTeleportLocId = "mc-teleporter-nothing";

    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly MCPairLinkSystem _mcPair = null!;
    [Dependency] private readonly MCDeploySystem _mcDeploy = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCTeleporterComponent, InteractHandEvent>(OnInteract);
    }

    private void OnInteract(Entity<MCTeleporterComponent> entity, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!_mcDeploy.Deployed(entity))
            return;

        if (entity.Comp.TeleportNext > _timing.CurTime)
        {
            var time = (int) double.Ceiling((entity.Comp.TeleportNext - _timing.CurTime).TotalSeconds);
            _popup.PopupClient(Loc.GetString(RechargingLocId, ("seconds", time)), entity, args.User, PopupType.MediumCaution);
            return;
        }

        if (!_mcPair.TryGetLink(entity, out var linked))
        {
            Fail(entity, NoLinkLocId, args.User);
            return;
        }

        if (!TryComp<MCTeleporterComponent>(linked, out var linkedComp) || !_mcDeploy.Deployed(linked))
        {
            Fail(entity, InvalidLinkLocId, args.User);
            return;
        }

        var teleported = TeleportContents(entity, linked);
        if (teleported == 0)
        {
            Fail(entity, NothingToTeleportLocId, args.User);
            return;
        }

        entity.Comp.TeleportNext = _timing.CurTime + entity.Comp.TeleportCooldown;
        linkedComp.TeleportNext = _timing.CurTime + linkedComp.TeleportCooldown;

        Dirty(entity);
        Dirty(linked, linkedComp);

        _audio.PlayPredicted(entity.Comp.EffectSoundTeleport, entity, args.User);

        args.Handled = true;
    }

    private int TeleportContents(Entity<MCTeleporterComponent> source, EntityUid target)
    {
        var coords = Transform(source).Coordinates;
        var targetCoords = Transform(target).Coordinates;
        var count = 0;

        foreach (var entity in _lookup.GetEntitiesIntersecting(coords))
        {
            if (entity == source.Owner || entity == target)
                continue;

            if (Transform(entity).Anchored)
                continue;

            if (!HasComp<MobStateComponent>(entity))
                continue;

            _transform.SetCoordinates(entity, targetCoords);
            count++;
        }

        return count;
    }

    private void Fail(
        Entity<MCTeleporterComponent> entity,
        LocId message,
        EntityUid user)
    {
        _popup.PopupClient(Loc.GetString(message), entity, user, PopupType.MediumCaution);
        _audio.PlayPredicted(entity.Comp.EffectSoundFail, entity, user);
    }
}
