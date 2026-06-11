using Content.Shared._MC.Xeno.Constructions.PheromoneTowers.UI;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.Structures.PheromoneTowers.Components;
using Content.Shared._RMC14.Xenonids.Pheromones;
using Content.Shared.Interaction;

namespace Content.Shared._MC.Xeno.Structures.PheromoneTowers;

public sealed class MCXenoStructurePheromoneTowerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _xenoHive = null!;

    private EntityQuery<XenoPheromonesObjectComponent> _xenoPheromonesObjectQuery;
    private EntityQuery<XenoActivePheromonesComponent> _xenoPheromonesActiveQuery;

    public override void Initialize()
    {
        _xenoPheromonesObjectQuery = GetEntityQuery<XenoPheromonesObjectComponent>();
        _xenoPheromonesActiveQuery = GetEntityQuery<XenoActivePheromonesComponent>();

        SubscribeLocalEvent<MCXenoStructurePheromoneTowerComponent, InteractHandEvent>(OnInteractHand);

        Subs.BuiEvents<MCXenoStructurePheromoneTowerComponent>(MCXenoStructurePheromoneTowerSelectUI.Key,
            sub =>
            {
                sub.Event<MCXenoStructurePheromoneTowerSelectUIMessage>(OnMessageSelect);
            }
        );
    }

    private void OnInteractHand(Entity<MCXenoStructurePheromoneTowerComponent> entity, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!_xenoHive.FromSameHive(entity.Owner, args.User))
            return;

        _userInterface.TryOpenUi(entity.Owner, MCXenoStructurePheromoneTowerSelectUI.Key, args.User);
        args.Handled = true;
    }

    private void OnMessageSelect(Entity<MCXenoStructurePheromoneTowerComponent> entity, ref MCXenoStructurePheromoneTowerSelectUIMessage args)
    {
        entity.Comp.SelectedType = args.SelectedType;
        DirtyField(entity, entity.Comp, nameof(MCXenoStructurePheromoneTowerComponent.SelectedType));

        if (_xenoPheromonesObjectQuery.TryComp(entity, out var pheromonesObjectComponent))
        {
            pheromonesObjectComponent.Pheromones = args.SelectedType;
            Dirty(entity, entity.Comp);
        }

        if (_xenoPheromonesActiveQuery.TryComp(entity, out var pheromonesActiveComponent))
        {
            pheromonesActiveComponent.Pheromones = args.SelectedType;
            Dirty(entity, entity.Comp);
        }

        _appearance.SetData(entity, MCXenoStructurePheromoneTowerLayers.Layer, args.SelectedType);
    }
}
