using Content.Server.Shuttles.Events;
using Content.Shared._MC.Operation;

namespace Content.Server._MC.Operation;

public sealed class MCOperationStartOnLandSystem : EntitySystem
{
    [Dependency] private readonly MCOperationSystem _mcOperation = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCOperationStartOnLandComponent, FTLCompletedEvent>(OnFtlCompleted);
    }

    private void OnFtlCompleted(Entity<MCOperationStartOnLandComponent> entity, ref FTLCompletedEvent args)
    {
        _mcOperation.Start();
    }
}
