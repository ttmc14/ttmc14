using System.Linq;
using Content.Shared._MC.Nuke.Bomb.Components;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Nuke.Bomb.UI;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Nuke.Bomb;

public sealed class MCNukeSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCNukeComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<MCNukeComponent, EntInsertedIntoContainerMessage>(OnRefreshReady);
        SubscribeLocalEvent<MCNukeComponent, EntRemovedFromContainerMessage>(OnRefreshReady);

        SubscribeLocalEvent<MCNukeComponent, MCNukeAnchorBuiMessage>(OnAnchorMessage);
        SubscribeLocalEvent<MCNukeComponent, MCNukeSafetyBuiMessage>(OnSafetyMessage);
        SubscribeLocalEvent<MCNukeComponent, MCNukeActiveBuiMessage>(OnActiveMessage);
    }

    private void OnStartup(Entity<MCNukeComponent> ent, ref ComponentStartup args)
    {
        RefreshUi(ent);
    }

    private void OnAnchorMessage(Entity<MCNukeComponent> ent, ref MCNukeAnchorBuiMessage args)
    {
        if (!ent.Comp.Ready)
            return;

        var transform = Transform(ent);
        var value = !transform.Anchored;

        try
        {
            if (value)
            {
                _transform.AnchorEntity(ent);
                return;
            }

            _transform.Unanchor(ent);
        }
        finally
        {
            RefreshUi(ent);
        }
    }

    private void OnSafetyMessage(Entity<MCNukeComponent> ent, ref MCNukeSafetyBuiMessage args)
    {
        if (!ent.Comp.Ready)
            return;

        ent.Comp.Safety = !ent.Comp.Safety;
        RefreshUi(ent);
    }

    private void OnActiveMessage(Entity<MCNukeComponent> ent, ref MCNukeActiveBuiMessage args)
    {
        if (!ent.Comp.Ready)
            return;

        var transform = Transform(ent);
        if (ent.Comp.Safety || !transform.Anchored)
            return;

        ent.Comp.Activated = !ent.Comp.Activated;

        // TODO: todo lol
        if (!ent.Comp.Activated)
            return;

        var ev = new MCNukeExplodedEvent();
        RaiseLocalEvent(ev);
    }

    private void OnRefreshReady<T>(Entity<MCNukeComponent> ent, ref T _)
    {
        ent.Comp.Ready = IsReady(ent);
        Dirty(ent);

        RefreshUi(ent);
    }

    private bool IsReady(Entity<MCNukeComponent> ent)
    {
        return ent.Comp.Slots.All(e => _container.TryGetContainer(ent, e, out var container) && container.Count >= 1);
    }

    private void RefreshUi(Entity<MCNukeComponent> ent)
    {
        var transform = Transform(ent);

        var state = new MCNukeBuiState(
            ent.Comp.Ready,
            ent.Comp.Safety,
            ent.Comp.Activated,
            transform.Anchored
        );

        _userInterface.SetUiState(ent.Owner, MCNukeUi.Key, state);
    }
}
