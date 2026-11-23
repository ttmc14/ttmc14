using System.Numerics;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Components;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Rounding;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

// ReSharper disable once CheckNamespace
namespace Content.Client._RMC14.Xenonids.Hud;

public sealed partial class XenoHudOverlay
{
    private readonly EntityQuery<MCXenoSunderComponent> _mcXenoSunderQuery;
    private readonly EntityQuery<MCXenoOverhealComponent> _mcXenoOverhealQuery;

    private void DrawOverheal(EntityUid uid, Vector2 healthPosition, DrawingHandleWorld handle)
    {
        if (!_mcXenoOverhealQuery.TryComp(uid, out var reserve) || reserve.Amount <= 0 || reserve.Maximum <= 0)
            return;

        // The fill occupies columns 0–1 in the reserve sprite and 3–4 in the health sprite.
        const float healthColumn = 3;
        var level = ContentHelpers.RoundToLevels((double) reserve.Amount, (double) reserve.Maximum, 11);
        var icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_MC/Interface/Xeno/hud.rsi"), $"over_plasma{level * 10}");
        var texture = _sprite.GetFrame(icon, _timing.CurTime);
        handle.DrawTexture(texture, healthPosition + new Vector2(healthColumn / EyeManager.PixelsPerMeter, 0));
    }

    private void UpdatePlasma(Entity<XenoComponent, SpriteComponent> ent, DrawingHandleWorld handle)
    {
        var (uid, xeno, sprite) = ent;

        if (!_xenoPlasmaQuery.TryComp(uid, out var comp) || comp.MaxPlasma <= 0)
            return;

        var stored = comp.Plasma;
        var plasma = stored.Double();
        if (_entity.TryGetComponent<MCXenoPlasmaComponent>(uid, out var reserve) && reserve.ShowBloodHud)
        {
            DrawBar(GetBloodHudState(plasma, comp.MaxPlasma), xeno, sprite, handle, "/Textures/_MC/Interface/Xeno/fury.rsi");
            return;
        }

        var plasmaMax = comp.MaxPlasma;

        var regenLimit = comp.PlasmaRegenLimit == -1
            ? plasmaMax
            : comp.PlasmaRegenLimit;

        regenLimit = Math.Clamp(regenLimit, 0, plasmaMax);

        var plasmaLevel = regenLimit > 0 ? ContentHelpers.RoundToLevels(plasma, regenLimit, 11) : 0;
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

    /// <summary>
    /// Selects the world HUD sprite for the blood reserve as a percentage of full capacity.
    /// </summary>
    public static string GetBloodHudState(double blood, int capacity)
    {
        if (capacity <= 0)
            return "fury0";

        var fraction = Math.Clamp(blood / capacity, 0, 1);
        var level = (int) Math.Round(fraction * 10, MidpointRounding.AwayFromZero);
        return $"fury{level * 10}";
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

    private void DrawBar(string state, XenoComponent xeno, SpriteComponent sprite, DrawingHandleWorld handle,
        string path = "/Textures/_RMC14/Interface/xeno_hud.rsi")
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
