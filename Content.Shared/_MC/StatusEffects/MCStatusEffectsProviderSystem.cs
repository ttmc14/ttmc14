using Content.Shared._MC.Armor.Events;
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
    }
}
