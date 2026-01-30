using Content.Shared._MC.Operation.Events;
using Content.Shared.Doors.Systems;

namespace Content.Shared._MC.Operation;

public sealed class MCOperationPodlockSystem : EntitySystem
{
    [Dependency] private readonly SharedDoorSystem _doors = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCOperationStartEvent>(OnStart);
    }

    private void OnStart(ref MCOperationStartEvent args)
    {
        var query = EntityQueryEnumerator<MCOperationPodlockComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            _doors.TryOpen(uid);
        }
    }
}
