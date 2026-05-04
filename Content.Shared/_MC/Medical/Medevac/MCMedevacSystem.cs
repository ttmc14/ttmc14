using Content.Shared._MC.Areas;
using Content.Shared._MC.Chat;
using Content.Shared._MC.Deploy;
using Content.Shared._MC.Linking.Pair;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Medical.Medevac;

public sealed class MCMedevacSystem : EntitySystem
{
    private static readonly LocId NoBeaconLocId = "mc-medevac-no-beacon";
    private static readonly LocId RechargingLocId = "mc-medevac-recharging";
    private static readonly LocId BeaconNotPlantedLocId = "mc-medevac-beacon-not-planted";
    private static readonly LocId AlertLocId = "mc-medevac-alert";
    private static readonly LocId SafetyNoBuckledLocId = "mc-medevac-safety-no-buckled";
    private static readonly LocId ActivateVisibleLocId = "mc-medevac-activate-visible";

    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedBuckleSystem _buckle = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCAreasSystem _mcArea = null!;
    [Dependency] private readonly MCSharedChatSystem _mcChat = null!;
    [Dependency] private readonly MCDeploySystem _mcDeploy = null!;
    [Dependency] private readonly MCPairLinkSystem _mcPairLink = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCMedevacComponent, InteractHandEvent>(OnInteractHand);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCMedevacComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var entity = new Entity<MCMedevacComponent>(uid, component);

            if (!entity.Comp.Active)
                continue;

            if (entity.Comp.ActiveNext > _timing.CurTime)
                continue;

            entity.Comp.Active = false;
            DirtyField(entity, entity.Comp, nameof(MCMedevacComponent.Active));

            if (TryTeleport(entity))
            {
                entity.Comp.EvacNext = _timing.CurTime + entity.Comp.EvacCooldown;
                DirtyField(entity, entity.Comp, nameof(MCMedevacComponent.EvacNext));
                continue;
            }

            _mcChat.TrySendInGameICSpeakMessage(uid, Loc.GetString(SafetyNoBuckledLocId, ("this", uid)), true);

            if (_net.IsServer)
                _audio.PlayPvs(entity.Comp.FailSound, entity);
        }
    }

    private void OnInteractHand(Entity<MCMedevacComponent> entity, ref InteractHandEvent args)
    {
        if (args.Handled || !TryComp<StrapComponent>(entity, out var strapComponent) || strapComponent.BuckledEntities.Count == 0 || entity.Comp.Active)
            return;

        if (entity.Comp.InteractNext > _timing.CurTime)
            return;

        entity.Comp.InteractNext = _timing.CurTime + entity.Comp.InteractCooldown;
        DirtyField(entity, entity.Comp, nameof(MCMedevacComponent.InteractNext));

        args.Handled = true;

        if (!_mcPairLink.TryGetLink(entity, out var link))
        {
            _popup.PopupClient(Loc.GetString(NoBeaconLocId, ("this", entity)), entity, args.User, PopupType.MediumCaution);
            _audio.PlayPredicted(entity.Comp.FailSound, entity, args.User);
            return;
        }

        if (entity.Comp.EvacNext > _timing.CurTime)
        {
            _popup.PopupClient(Loc.GetString(RechargingLocId, ("this", entity), ("seconds", (int) (entity.Comp.EvacNext - _timing.CurTime).TotalSeconds)), entity, args.User, PopupType.MediumCaution);
            _audio.PlayPredicted(entity.Comp.FailSound, entity, args.User);
            return;
        }

        if (!_mcDeploy.Deployed(link))
        {
            _popup.PopupClient(Loc.GetString(BeaconNotPlantedLocId, ("this", entity)), entity, args.User, PopupType.MediumCaution);
            _audio.PlayPredicted(entity.Comp.FailSound, entity, args.User);
            return;
        }

        _mcChat.TrySendInGameICSpeakMessage(entity, Loc.GetString(ActivateVisibleLocId, ("this", entity), ("user", args.User)), true);
        Active(entity);
    }

    private void Active(Entity<MCMedevacComponent> entity)
    {
        entity.Comp.Active = true;
        entity.Comp.ActiveNext = _timing.CurTime + entity.Comp.ActiveTime;
        DirtyFields(entity, entity.Comp, null, nameof(MCMedevacComponent.Active), nameof(MCMedevacComponent.ActiveNext));

        _audio.PlayPredicted(entity.Comp.ActivateSound, entity, entity);
    }

    private bool TryTeleport(Entity<MCMedevacComponent> entity)
    {
        if (!TryComp<StrapComponent>(entity, out var strapComponent) || strapComponent.BuckledEntities.Count == 0)
            return false;

        if (!_mcPairLink.TryGetLink(entity, out var link))
            return false;

        var coordinates = Transform(link).Coordinates;

        _mcArea.GetAreaCoordsMessage(link, out var coords, out var areaName);
        foreach (var uid in strapComponent.BuckledEntities)
        {
            _buckle.Unbuckle(uid, null);
            _transform.SetCoordinates(uid, coordinates);

            var message = Loc.GetString(AlertLocId,
                ("target", uid),
                ("x", coords.X),
                ("y", coords.Y),
                ("area", areaName)
            );

            _mcChat.TrySendInGameICSpeakMessage(entity, message, true);
        }

        if (_net.IsServer)
            _audio.PlayPvs(entity.Comp.TeleportSound, entity);

        return true;
    }
}
