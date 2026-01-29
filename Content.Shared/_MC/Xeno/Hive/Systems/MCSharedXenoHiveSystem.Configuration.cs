using Content.Shared._MC.Xeno.Hive.Components;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract partial class MCSharedXenoHiveSystem
{
    public void SetConfiguration(Entity<MCXenoHiveComponent?> entity, MCXenoHiveConfiguration configuration)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Configuration = configuration;
        Dirty(entity);
    }
}
