namespace Content.Shared._MC.Flammable;

public abstract class MCSharedFlammableSystem : EntitySystem
{
    public virtual bool OnFire(EntityUid uid)
    {
        return false;
    }

    public virtual void AdjustFireStacks(EntityUid uid, float stacks, bool ignite = false)
    {
    }
}
