using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.Structures.PheromoneTowers.Components;
using Content.Shared._MC.Xeno.Structures.PheromoneTowers.UI;
using Content.Shared._RMC14.Xenonids.Pheromones;
using Content.Shared.Interaction;

namespace Content.Shared._MC.Xeno.Structures.PheromoneTowers;

public sealed class MCXenoStructurePheromoneTowerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _xenoHive = null!;

    private EntityQuery<XenoPheromonesObjectComponent> _xenoPheromonesObjectQuery;
    private EntityQuery<XenoActivePheromonesComponent> _xenoPheromonesActiveQuery;
    private EntityQuery<SharedPointLightComponent> _pointLightQuery;

    public override void Initialize()
    {
        _xenoPheromonesObjectQuery = GetEntityQuery<XenoPheromonesObjectComponent>();
        _xenoPheromonesActiveQuery = GetEntityQuery<XenoActivePheromonesComponent>();
        _pointLightQuery = GetEntityQuery<SharedPointLightComponent>();

        SubscribeLocalEvent<MCXenoStructurePheromoneTowerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoStructurePheromoneTowerComponent, InteractHandEvent>(OnInteractHand);

        Subs.BuiEvents<MCXenoStructurePheromoneTowerComponent>(MCXenoStructurePheromoneTowerSelectUI.Key,
            sub =>
            {
                sub.Event<MCXenoStructurePheromoneTowerSelectUIMessage>(OnMessageSelect);
            }
        );
    }

    private void OnStartup(Entity<MCXenoStructurePheromoneTowerComponent> entity, ref ComponentStartup args)
    {
        Select(entity, entity.Comp.Selected);
    }

    private void OnInteractHand(Entity<MCXenoStructurePheromoneTowerComponent> entity, ref InteractHandEvent args)
    {
        if (args.Handled || !_xenoHive.FromSameHive(entity.Owner, args.User))
            return;

        args.Handled = true;

        _userInterface.TryOpenUi(entity.Owner, MCXenoStructurePheromoneTowerSelectUI.Key, args.User);
    }

    private void OnMessageSelect(Entity<MCXenoStructurePheromoneTowerComponent> entity, ref MCXenoStructurePheromoneTowerSelectUIMessage args)
    {
        Select(entity, args.SelectedType);
    }

    private void Select(Entity<MCXenoStructurePheromoneTowerComponent> entity, XenoPheromones type)
    {
        entity.Comp.Selected = type;

        if (_xenoPheromonesObjectQuery.TryComp(entity, out var pheromonesObjectComponent))
            pheromonesObjectComponent.Pheromones = type;

        if (_xenoPheromonesActiveQuery.TryComp(entity, out var pheromonesActiveComponent))
            pheromonesActiveComponent.Pheromones = type;

        _pointLight.SetColor(entity, entity.Comp.TypeColor[type]);
        _appearance.SetData(entity, MCXenoStructurePheromoneTowerLayers.Layer, type);
    }
}
