using Content.Shared._MC.Armor.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._MC.StatusEffects;

public sealed class MCStatusEffectsProviderSystem : EntitySystem
{
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectContainerComponent, MCArmorGetEvent>(_statusEffects.RelayEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, MCArmorModifyEvent>(_statusEffects.RelayEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, RefreshMovementSpeedModifiersEvent>(RelayEvent);
    }

    private void RelayEvent<TEvent>(Entity<StatusEffectContainerComponent> entity, ref TEvent args) where TEvent : class
    {
        _statusEffects.RelayEvent(entity, args);
    }
}
