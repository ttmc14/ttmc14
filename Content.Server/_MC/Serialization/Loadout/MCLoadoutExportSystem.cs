using Content.Shared._MC.Serialization.Loadout;

namespace Content.Server._MC.Serialization.Loadout;

public sealed class MCLoadoutExportSystem : EntitySystem
{
    [Dependency] private readonly MCLoadoutSerializerSystem _serializer = null!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<MCLoadoutExportRequest>(OnExportRequest);
    }

    private void OnExportRequest(MCLoadoutExportRequest message, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is { } player)
            RaiseNetworkEvent(new MCLoadoutExportResponse(_serializer.BuildEntity(player)), args.SenderSession);
    }
}
