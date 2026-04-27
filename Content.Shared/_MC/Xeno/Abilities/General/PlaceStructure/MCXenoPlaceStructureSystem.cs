using Content.Shared._MC.Xeno.Construction;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Xeno.Abilities.General.PlaceStructure;

public sealed class MCXenoPlaceStructureSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    [Dependency] private readonly MCXenoConstructionSystem _mcXenoConstruction = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPlaceStructureComponent, MCXenoPlaceStructureInstantActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPlaceStructureComponent, MCXenoPlaceStructureDoAfterEvent>(OnActionDoAfter);
    }

    private void OnAction(Entity<MCXenoPlaceStructureComponent> entity, ref MCXenoPlaceStructureInstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (!CanPlace(entity, args.Structure))
            return;

        if (args.Structure.Delay == TimeSpan.Zero)
        {
            Place(entity, args.Structure);
            return;
        }

        var ev = new MCXenoPlaceStructureDoAfterEvent(args.Structure);
        var doAfter = new DoAfterArgs(EntityManager, entity, args.Structure.Delay, ev, entity)
        {
            NeedHand = true,
            BreakOnMove = true,
            RequireCanInteract = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnActionDoAfter(Entity<MCXenoPlaceStructureComponent> entity, ref MCXenoPlaceStructureDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!CanPlace(entity, args.Structure))
            return;

        Place(entity, args.Structure);
    }

    private void Place(Entity<MCXenoPlaceStructureComponent> entity, MCXenoPlaceStructurePayload structure)
    {
        if (Net.IsServer)
        {
            var instance = Spawn(structure.StructureProtoId, Transform(entity).Coordinates);
            MCXenoHive.SetSameHive(entity.Owner, instance);
        }

        _mcXenoPlasma.RemovePlasma(entity, structure.PlasmaCost);

        ActionStartUseDelay<MCXenoPlaceStructureInstantActionEvent>(entity);
    }

    private bool CanPlace(Entity<MCXenoPlaceStructureComponent> entity, MCXenoPlaceStructurePayload structure)
    {
        var coordinates = Transform(entity).Coordinates;

        if (!_mcXenoConstruction.CanPlace(entity, coordinates, out _, needsWeeds: structure.RequireWeeds))
            return false;

        if (!_mcXenoPlasma.HasPlasma(entity, structure.PlasmaCost))
            return false;

        return true;
    }
}
