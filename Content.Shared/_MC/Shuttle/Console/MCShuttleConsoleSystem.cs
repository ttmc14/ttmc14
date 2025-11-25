using Content.Shared._MC.Shuttle.Console.UI;

namespace Content.Shared._MC.Shuttle.Console;

public sealed class MCShuttleConsoleSystem : EntitySystem
{
    [Dependency] private readonly MCShuttleSystem _mcShuttle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCShuttleConsoleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCShuttleConsoleComponent, MCShuttleConsoleEvacuateBuiMessage>(OnConsoleEvacuationMessage);
    }

    private void OnStartup(Entity<MCShuttleConsoleComponent> entity, ref ComponentStartup args)
    {
        var gridUid = Transform(entity).GridUid;
        entity.Comp.Shuttle = HasComp<MCShuttleComponent>(gridUid) ? gridUid : null;
        Dirty(entity);
    }

    private void OnConsoleEvacuationMessage(Entity<MCShuttleConsoleComponent> entity, ref MCShuttleConsoleEvacuateBuiMessage args)
    {
        if (entity.Comp.Shuttle is not {} shuttleEntity)
            return;

        if (!TryComp<MCShuttleComponent>(shuttleEntity, out var shuttleComponent))
            return;

        _mcShuttle.Evacuate((shuttleEntity, shuttleComponent));
    }
}
