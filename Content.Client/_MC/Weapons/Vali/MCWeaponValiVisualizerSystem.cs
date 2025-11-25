using Content.Shared._MC.Weapon.Vali;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Wieldable.Components;
using Robust.Client.GameObjects;

namespace Content.Client._MC.Weapons.Vali;

public sealed class MCWeaponValiVisualizerSystem : VisualizerSystem<MCWeaponValiComponent>
{
    [Dependency] private readonly RMCReagentSystem _rmcReagent = null!;

    protected override void OnAppearanceChange(EntityUid uid, MCWeaponValiComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (args.Sprite is null)
            return;

        var sprite = new Entity<SpriteComponent?>(uid, args.Sprite);

        if (!SpriteSystem.LayerMapTryGet(sprite, MCWeaponValiVisualLayers.Blade, out var bladeLayer, false))
            return;

        if (!AppearanceSystem.TryGetData<string>(uid, MCWeaponValiVisuals.ReagentId, out var value, args.Component))
            return;

        if (!_rmcReagent.TryIndex(value, out var reagent))
        {
            SpriteSystem.LayerSetColor(sprite, bladeLayer, Color.FromHex("#008000"));
            return;
        }

        SpriteSystem.LayerSetColor(sprite, bladeLayer, reagent.SubstanceColor);
    }
}
