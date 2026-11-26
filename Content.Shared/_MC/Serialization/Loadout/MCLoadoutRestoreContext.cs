using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Serialization.Loadout;

public sealed class MCLoadoutRestoreContext(MCLoadoutSerializerSystem serializer, EntityUid user, Func<EntProtoId, EntityUid?> purchase)
{
    public EntityUid User => user;

    public EntityUid? Purchase(MCLoadoutItem item) => purchase(item.ProtoId);

    public EntityUid? Vend(MCLoadoutItem item)
    {
        var uid = Purchase(item);
        if (uid is { } spawned)
            Restore(spawned, item);

        return uid;
    }

    public void Restore(EntityUid uid, MCLoadoutItem item)
        => serializer.RestoreItem(uid, item, this);
}
