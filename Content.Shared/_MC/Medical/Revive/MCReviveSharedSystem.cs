namespace Content.Shared._MC.Medical.Revive;

public abstract class MCReviveSharedSystem : EntitySystem
{
    public abstract bool SendReviveRequest(EntityUid targetUid);
}
