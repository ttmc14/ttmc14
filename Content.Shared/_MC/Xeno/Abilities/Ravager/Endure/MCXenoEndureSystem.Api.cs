namespace Content.Shared._MC.Xeno.Abilities.Ravager.Endure;

public sealed partial class MCXenoEndureSystem
{
    public void ExtendDuration(EntityUid uid, TimeSpan duration)
    {
        if (!_queryActive.TryComp(uid, out var component))
            return;

        component.EndTime += duration;
    }
}
