using System.Numerics;
using Content.Shared._MC;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Configuration;

namespace Content.Client._MC;

public sealed class MCUserInterfaceUtilitiesSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;
    [Dependency] private readonly IClyde _clyde = null!;
    [Dependency] private readonly IEyeManager _eye = null!;
    [Dependency] private readonly IInputManager _input = null!;
    [Dependency] private readonly IPlayerManager _player = null!;

    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [PublicAPI]
    public Vector2 GetRadialPosition(EntityUid uid)
    {
        return _configuration.GetCVar(MCConfigVars.UIRadialSpawnAtMouse)
            ? GetRadialPositionMouse()
            : GetRadialPositionEntity(uid);
    }

    [PublicAPI]
    public Vector2 GetRadialPositionEntity(EntityUid uid)
    {
        var screenSize = _clyde.ScreenSize;
        var position = _input.MouseScreenPosition.Position / screenSize;

        if (_player.LocalEntity is { } ent)
            position = _eye.WorldToScreen(_transform.GetMapCoordinates(ent).Position) / screenSize;

        if (TryComp<EyeComponent>(uid, out var eyeComp) && eyeComp.Target is not null)
            position = _eye.WorldToScreen(_transform.GetMapCoordinates((EntityUid) eyeComp.Target).Position) / screenSize;

        return position;
    }

    [PublicAPI]
    public Vector2 GetRadialPositionMouse()
    {
        return _input.MouseScreenPosition.Position / _clyde.ScreenSize;
    }
}
