using Content.Client.CombatMode;
using Content.Client.Gameplay;
using Content.Shared._MC.Weapons.Range.Delayed;
using Content.Shared._MC.Weapons.Range.Delayed.Events;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._MC.Weapons.Range.Delayed;

public sealed class MCWeaponRangeDelayedSystem : MCWeaponRangeDelayedSharedSystem
{
    [Dependency] private readonly IStateManager _state = null!;
    [Dependency] private readonly IEyeManager _eye = null!;
    [Dependency] private readonly IInputManager _inputManager = null!;
    [Dependency] private readonly IMapManager _mapManager  = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly IOverlayManager _overlay = null!;

    [Dependency] private readonly CombatModeSystem _combatMode = null!;
    [Dependency] private readonly InputSystem _input = null!;
    [Dependency] private readonly TransformSystem _transform = null!;
    [Dependency] private readonly MapSystem _map = null!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new MCWeaponRangeDelayedOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<MCWeaponRangeDelayedOverlay>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_player.LocalEntity is not { } entityUid || _player.LocalSession is not { } session)
            return;

        if (!TryGetGun(entityUid, out var gunUid, out var gun, out _))
            return;

        if (!_combatMode.IsInCombatMode(entityUid))
            return;

        var keyState = _input.CmdStates.GetState(EngineKeyFunctions.Use);
        if (keyState != BoundKeyState.Down)
        {
            var evEnd = new MCWeaponRangeDelayedRequestStopEvent
            {
                Gun = GetNetEntity(gunUid),
            };

            RaisePredictiveEvent(evEnd);
            return;
        }

        var mousePosition = _eye.PixelToMap(_inputManager.MouseScreenPosition);
        if (mousePosition.MapId == MapId.Nullspace)
            return;

        var coordinates = _mapManager.TryFindGridAt(mousePosition, out var gridUid, out _)
            ? _transform.ToCoordinates(gridUid, mousePosition)
            : _transform.ToCoordinates(_map.GetMap(mousePosition.MapId), mousePosition);

        var target = _state.CurrentState is GameplayStateBase screen
            ? GetNetEntity(screen.GetClickedEntity(mousePosition))
            : null;

        var evStart = new MCWeaponRangeDelayedRequestStartEvent
        {
            Target = target,
            Coordinates = GetNetCoordinates(coordinates),
            Gun = GetNetEntity(gunUid),
            LastRealTick = RMCLagCompensation.GetLastRealTick(null),
        };

        RaisePredictiveEvent(evStart);
    }
}
