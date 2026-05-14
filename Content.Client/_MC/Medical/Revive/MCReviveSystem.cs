using Content.Shared._MC.Medical.Revive;

namespace Content.Client._MC.Medical.Revive;

public sealed class MCReviveSystem : MCReviveSharedSystem
{
    public override bool SendReviveRequest(EntityUid targetUid)
    {
        return false;
    }
}
