using Content.Shared._MC.Engineering.Linking.Pair.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Shared._MC.Engineering.Linking.Pair;

public sealed class MCPairLinkInteractLinkedSystem : EntitySystem
{
    private static readonly LocId AlreadyLinkedLocId = "mc-pair-link-already-linked";
    private static readonly LocId LinkedLocId = "mc-pair-link-linked";

    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly MCPairLinkSystem _mcPairLink = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCPairLinkInteractLinkedComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<MCPairLinkInteractLinkedComponent> entity, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } targetUid)
            return;

        if (!_entityWhitelist.IsWhitelistPass(entity.Comp.Whitelist, targetUid))
            return;

        args.Handled = true;

        if (_mcPairLink.IsLinkTo(entity, targetUid))
        {
            _popup.PopupClient(Loc.GetString(AlreadyLinkedLocId), targetUid, args.User, PopupType.Medium);
            return;
        }

        _mcPairLink.CreateLink(entity, targetUid);
        _popup.PopupClient(Loc.GetString(LinkedLocId), targetUid, args.User, PopupType.Medium);
    }
}
