using Content.Shared._MC.Armor.Core.Events;
using Content.Shared._MC.Xeno.Visuals;
using Content.Shared.Movement.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.Agility;

public sealed class MCXenoAgilitySystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoAgilityComponent, MCXenoAgilityActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoAgilityActiveComponent, MapInitEvent>(OnActiveInit);
        SubscribeLocalEvent<MCXenoAgilityActiveComponent, ComponentRemove>(OnActiveRemove);

        SubscribeLocalEvent<MCXenoAgilityActiveComponent, RefreshMovementSpeedModifiersEvent>(OnActiveRefreshSpeed);
        SubscribeLocalEvent<MCXenoAgilityActiveComponent, MCArmorGetEvent>(OnActiveArmorGet);
    }

    public void Disable(EntityUid uid)
    {
        RemComp<MCXenoAgilityActiveComponent>(uid);
    }

    private void OnAction(Entity<MCXenoAgilityComponent> entity, ref MCXenoAgilityActionEvent args)
    {
        if (args.Handled || RemComp<MCXenoAgilityActiveComponent>(entity) || !TryUseAction(entity, args.Action))
            return;

        args.Handled = true;

        var agilityComponent = new MCXenoAgilityActiveComponent
        {
            ArmorFlat = entity.Comp.ArmorFlat,
            SpeedModifier = entity.Comp.SpeedModifier,
        };

        AddComp(entity, agilityComponent);
        Dirty(entity.Owner, agilityComponent);
    }

    private void OnActiveInit(Entity<MCXenoAgilityActiveComponent> entity, ref MapInitEvent args)
    {
        SetEnabled(entity, true);
    }

    private void OnActiveRemove(Entity<MCXenoAgilityActiveComponent> entity, ref ComponentRemove args)
    {
        SetEnabled(entity, false);
    }

    private static void OnActiveRefreshSpeed(Entity<MCXenoAgilityActiveComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(entity.Comp.SpeedModifier);
    }

    private static void OnActiveArmorGet(Entity<MCXenoAgilityActiveComponent> entity, ref MCArmorGetEvent args)
    {
        args.SoftArmor += entity.Comp.ArmorFlat;
    }

    private void SetEnabled(EntityUid uid, bool enabled)
    {
        _appearance.SetData(uid, MCXenoVisualLayers.Agility, enabled);
        _movementSpeed.RefreshMovementSpeedModifiers(uid);

        ActionSetToggled<MCXenoAgilityActionEvent>(uid, enabled);
    }
}
