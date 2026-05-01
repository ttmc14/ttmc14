using Content.Shared._MC.Research.Misc;
using Content.Shared.Interaction;

namespace Content.Shared._MC.Research.Machines.XenoAnalyzer;

// TODO xd
public sealed class MCMachineXenoAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly MCResearchableResourceSystem _mcResearchableResource = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCMachineXenoAnalyzerComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<MCMachineXenoAnalyzerComponent> entity, ref InteractUsingEvent args)
    {
        if (!Transform(entity).Anchored)
            return;

        if (!TryComp<MCResearchableResourceComponent>(args.Used, out var usedComponent))
            return;

        var coordinates = Transform(entity).Coordinates;
        _mcResearchableResource.SpawnResearchRewards((args.Used, usedComponent), coordinates);

        PredictedDel(args.Used);
    }
}
