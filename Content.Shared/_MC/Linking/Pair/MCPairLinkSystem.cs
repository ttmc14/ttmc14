using Content.Shared._MC.Areas;
using Content.Shared._MC.Linking.Pair.Components;
using Content.Shared.Examine;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Linking.Pair;

public sealed class MCPairLinkSystem : EntitySystem
{
    [Dependency] private readonly MCAreasSystem _mcArea = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCPairLinkComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCPairLinkComponent, ExaminedEvent>(OnExamined);
    }

    private void OnShutdown(Entity<MCPairLinkComponent> entity, ref ComponentShutdown args)
    {
        if (entity.Comp.LinkedEntityUid is not { } linkedEntityUid)
            return;

        entity.Comp.LinkedEntityUid = null;

        if (!TryComp<MCPairLinkComponent>(linkedEntityUid, out var linkedComponent))
            return;

        linkedComponent.LinkedEntityUid = null;
        RemComp<MCPairLinkComponent>(linkedEntityUid);
    }

    private void OnExamined(Entity<MCPairLinkComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.LinkedEntityUid is not { } linkedEntityUid)
            return;

        _mcArea.GetAreaCoordsMessage(linkedEntityUid, out var coordinates, out var areaName);

        var message = new FormattedMessage();

        message.AddMarkupOrThrow(Loc.GetString("mc-link-examined",
            ("linked",  linkedEntityUid),
            ("x", coordinates.X),
            ("y", coordinates.Y),
            ("area", areaName)
        ));

        args.PushMessage(message);
    }

    [PublicAPI]
    public bool IsLinkTo(EntityUid a, EntityUid b)
    {
        if (!TryGetLink(a, out var link))
            return false;

        return link == b;
    }

    [PublicAPI]
    public bool TryGetLink(EntityUid a, out EntityUid b)
    {
        b = default;

        var aComponent = EnsureComp<MCPairLinkComponent>(a);
        if (aComponent.LinkedEntityUid is not { } linkedEntityUid)
            return false;

        b = linkedEntityUid;
        return true;
    }

    [PublicAPI]
    public void CreateLink(EntityUid a, EntityUid b)
    {
        var aComponent = EnsureComp<MCPairLinkComponent>(a);
        var bComponent = EnsureComp<MCPairLinkComponent>(b);

        aComponent.LinkedEntityUid = b;
        bComponent.LinkedEntityUid = a;

        DirtyField(a, aComponent, nameof(MCPairLinkComponent.LinkedEntityUid));
        DirtyField(b, bComponent, nameof(MCPairLinkComponent.LinkedEntityUid));
    }

    [PublicAPI]
    public void RemoveLink(EntityUid a)
    {
        var aComponent = EnsureComp<MCPairLinkComponent>(a);
        if (aComponent.LinkedEntityUid is not { } linkedEntityUid)
            return;

        if (TerminatingOrDeleted(linkedEntityUid))
            return;

        RemoveLink(a, linkedEntityUid);
    }

    [PublicAPI]
    public void RemoveLink(EntityUid a, EntityUid b)
    {
        RemComp<MCPairLinkComponent>(a);
        RemComp<MCPairLinkComponent>(b);
    }
}
