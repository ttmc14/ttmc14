namespace Content.Shared._MC.Flammable;

public abstract class MCSharedFlammableSystem : EntitySystem
{
    public virtual bool OnFire(EntityUid uid)
    {
        return false;
    }

    public virtual void RemoveStacks(EntityUid uid, float stacks)
    {
    }
}
