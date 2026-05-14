using System.Numerics;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared.Rounding;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;
using Content.Shared._RMC14.Xenonids;

// ReSharper disable once CheckNamespace
namespace Content.Client._RMC14.Xenonids.Hud;

public sealed partial class XenoHudOverlay
{
    private readonly EntityQuery<MCXenoSunderComponent> _mcXenoSunderQuery;

    private void UpdatePlasma(Entity<XenoComponent, SpriteComponent> ent, DrawingHandleWorld handle)
    {
        var (uid, xeno, sprite) = ent;

        if (!_xenoPlasmaQuery.TryComp(uid, out var comp) || comp.MaxPlasma <= 0)
            return;

        var plasmaTemp = comp.Plasma;
        var plasma = double.Clamp(plasmaTemp.Double(), 0, comp.MaxPlasma);
        var plasmaMax = comp.MaxPlasma;

        var regenLimit = comp.PlasmaRegenLimit == -1
            ? plasmaMax
            : comp.PlasmaRegenLimit;

        regenLimit = Math.Clamp(regenLimit, 0, plasmaMax);

        var plasmaLevel = ContentHelpers.RoundToLevels(plasma, regenLimit, 11);
        DrawBar($"plasma{(plasmaLevel > 0 ? plasmaLevel * 10 : 0)}",
            xeno, sprite, handle,
            path: "/Textures/_MC/Interface/Xeno/hud.rsi");

        if (plasma <= regenLimit || regenLimit >= plasmaMax)
            return;

        var overPlasmaLevel = ContentHelpers.RoundToLevels(
            plasma - regenLimit,
            plasmaMax - regenLimit,
            11);

        DrawBar($"over_plasma{(overPlasmaLevel > 0 ? overPlasmaLevel * 10 : 0)}",
            xeno, sprite, handle,
            path: "/Textures/_MC/Interface/Xeno/hud.rsi");
    }

    private void UpdateSunder(Entity<XenoComponent, SpriteComponent> entity, DrawingHandleWorld handle)
    {
        var (uid, xeno, sprite) = entity;
        if (!_mcXenoSunderQuery.TryComp(uid, out var sunderComponent))
            return;

        var level = ContentHelpers.RoundToLevels(sunderComponent.Value, 100, 11);
        var name = level > 0 ? $"{level * 10}" : "0";
        DrawBar($"xenoarmor{name}", xeno, sprite, handle);
    }

    private void DrawBar(string state, XenoComponent xeno, SpriteComponent sprite, DrawingHandleWorld handle, string path = "/Textures/_RMC14/Interface/xeno_hud.rsi")
    {
        var icon = new SpriteSpecifier.Rsi(new ResPath(path), state);
        var texture = _sprite.GetFrame(icon, _timing.CurTime);

        var bounds = sprite.Bounds;
        var yOffset = (bounds.Height + sprite.Offset.Y) / 2f - (float) texture.Height / EyeManager.PixelsPerMeter * bounds.Height + xeno.HudOffset.Y;
        var xOffset = (bounds.Width + sprite.Offset.X) / 2f - (float) texture.Width / EyeManager.PixelsPerMeter * bounds.Width + xeno.HudOffset.X;

        var position = new Vector2(xOffset, yOffset);
        handle.DrawTexture(texture, position);
    }
}
