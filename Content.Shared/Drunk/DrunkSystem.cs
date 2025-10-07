using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Prototypes;

namespace Content.Shared.Drunk;

public abstract class SharedDrunkSystem : EntitySystem
{
    public static readonly EntProtoId DrunkKey = "Drunk";

    [Dependency] private readonly SharedStatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedSlurredSystem _slurredSystem = default!;

    public void TryApplyDrunkenness(EntityUid uid,
        float boozePower,
        bool applySlur = true,
        StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        if (TryComp<LightweightDrunkComponent>(uid, out var trait))
            boozePower *= trait.BoozeStrengthMultiplier;

        if (applySlur)
        {
            _slurredSystem.DoSlur(uid, TimeSpan.FromSeconds(boozePower), status);
        }

        if (!_statusEffectsSystem.HasStatusEffect(uid, DrunkKey))
        {
            _statusEffectsSystem.TryUpdateStatusEffectDuration(uid, DrunkKey, TimeSpan.FromSeconds(boozePower));
        }
        else
        {
            _statusEffectsSystem.TryAddTime(uid, DrunkKey, TimeSpan.FromSeconds(boozePower));
        }
    }

    public void TryRemoveDrunkenness(EntityUid uid)
    {
        _statusEffectsSystem.TryRemoveStatusEffect(uid, DrunkKey);
    }
    public void TryRemoveDrunkenessTime(EntityUid uid, double timeRemoved)
    {
        _statusEffectsSystem.TrySetTime(uid, DrunkKey, TimeSpan.FromSeconds(timeRemoved));
    }

}
