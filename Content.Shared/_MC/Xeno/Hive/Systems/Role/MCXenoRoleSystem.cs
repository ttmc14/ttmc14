using Content.Shared._MC.GameStates;
using Content.Shared._MC.Xeno.Hive.Events;
using Robust.Shared.Player;

namespace Content.Shared._MC.Xeno.Hive.Systems.Role;

public sealed class MCXenoRoleSystem : EntitySystem
{
    [Dependency] private readonly MCPvsOverrideSystem _pvsOverride = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, MCXenoHiveChangedEvent>(OnHiveChanged);
    }

    private void OnHiveChanged(Entity<ActorComponent> entity, ref MCXenoHiveChangedEvent args)
    {
        // We must add hive entity to PVS,
        // because we want prediction

        if (entity.Comp.PlayerSession is not { } session)
            return;

        if (args.OldHive is { } oldHive)
            _pvsOverride.RemoveForceSend(oldHive, session);

        if (args.Hive is { } newHive)
            _pvsOverride.AddForceSend(newHive, session);
    }
}
