using Content.Shared._MC.Xeno.Construction;
using Content.Shared._MC.Xeno.Construction.Blessings.Events;
using Content.Shared._MC.Xeno.Construction.Blessings.UI;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Blessings;

public sealed class MCXenoBlessingsSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly MCXenoConstructionSystem _mcXenoConstruction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoBlessingsComponent, MCXenoBlessingsActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoBlessingsComponent, MCXenoBlessingsChooseBuiMsg>(OnChooseMessage);
        SubscribeLocalEvent<MCXenoBlessingsComponent, MCXenoBlessingsBuildDoAfterEvent>(OnChooseDoAfter);
    }

    private void OnAction(Entity<MCXenoBlessingsComponent> entity, ref MCXenoBlessingsActionEvent args)
    {
        args.Handled = true;
        _ui.TryOpenUi(entity.Owner, MCXenoBlessingsUiKey.Key, entity);
    }

    private void OnChooseMessage(Entity<MCXenoBlessingsComponent> entity, ref MCXenoBlessingsChooseBuiMsg args)
    {
        if (!entity.Comp.Entries.Contains(args.Id))
            return;

        if (!_prototype.TryIndex(args.Id, out var entry))
            return;

        if (!_xenoHive.HasPsypointsFromOwner(entity, entry.CostType, entry.Cost))
            return;

        if (!_mcXenoConstruction.CanPlace(entity, Transform(entity).Coordinates, out var popupType))
        {
            _popup.PopupClient(Loc.GetString(popupType), entity, entity);
            return;
        }

        var ev = new MCXenoBlessingsBuildDoAfterEvent(args.Id, GetNetCoordinates(Transform(entity).Coordinates));
        var doAfter = new DoAfterArgs(EntityManager, entity, entry.Time, ev, entity)
        {
            NeedHand = true,
            BreakOnMove = true,
            RequireCanInteract = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnChooseDoAfter(Entity<MCXenoBlessingsComponent> entity, ref MCXenoBlessingsBuildDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (!entity.Comp.Entries.Contains(args.Id))
            return;

        if (!_prototype.TryIndex(args.Id, out var entry))
            return;

        if (!_xenoHive.HasPsypointsFromOwner(entity, entry.CostType, entry.Cost))
            return;

        if (!_mcXenoConstruction.CanPlace(entity, Transform(entity).Coordinates, out var popupType))
        {
            _popup.PopupClient(Loc.GetString(popupType), entity, entity);
            return;
        }

        _xenoHive.AddPsypointsFromOwner(entity, entry.CostType, -entry.Cost);

        if (!_net.IsServer)
            return;

        var structure = Spawn(entry.Entity, GetCoordinates(args.Coordinates));
        _xenoHive.SetSameHive(entity.Owner, structure);
    }
}
