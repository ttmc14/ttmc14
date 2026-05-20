using System.Numerics;
using Content.Shared._MC.Shuttles.FTL;
using Content.Shared._MC.Shuttles.Space;
using Content.Shared._MC.Shuttles.Tadpole.Components;
using Content.Shared._MC.Shuttles.Tadpole.UI;
using Robust.Shared.Map;

namespace Content.Shared._MC.Shuttles.Tadpole;

public sealed class MCShuttleTadpoleSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCShuttleFTLSharedSystem _mcFTL = null!;
    [Dependency] private readonly MCShuttleSpaceSystem _mcSpace = null!;

    public override void Initialize()
    {
        Subs.BuiEvents<MCShuttleTadpoleComponent>(MCShuttleTadpoleUI.Key,
            sub =>
            {
                sub.Event<MCShuttleTadpoleLandBuiMessage>(OnMessageLand);
                sub.Event<MCShuttleTadpoleReturnBuiMessage>(OnMessageReturn);
                sub.Event<MCShuttleTadpoleTakeOffBuiMessage>(OnMessageTakeOff);
            }
        );
    }

    private void OnMessageLand(Entity<MCShuttleTadpoleComponent> entity, ref MCShuttleTadpoleLandBuiMessage args)
    {

    }

    private void OnMessageReturn(Entity<MCShuttleTadpoleComponent> entity, ref MCShuttleTadpoleReturnBuiMessage args)
    {

    }

    private void OnMessageTakeOff(Entity<MCShuttleTadpoleComponent> entity, ref MCShuttleTadpoleTakeOffBuiMessage args)
    {
        _mcSpace.EnsureMap(entity.Comp.SpaceOrbit, out var mapId, out _);
        _mcFTL.FTLToCoordinates(Transform(entity).ParentUid, _transform.ToCoordinates(new MapCoordinates(Vector2.Zero, mapId)), Angle.Zero);
    }
}
