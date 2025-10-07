using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared.Speech.EntitySystems;

public abstract class SharedStutteringSystem : EntitySystem
{
    public static readonly EntProtoId StutterKey = "Stutter";

    [Dependency] private readonly SharedStatusEffectsSystem _statusEffectsSystem = default!;

    // For code in shared... I imagine we ain't getting accent prediction anytime soon so let's not bother.
    public virtual void DoStutter(EntityUid uid, TimeSpan time, bool refresh, StatusEffectsComponent? status = null)
    {
    }

    public virtual void DoRemoveStutterTime(EntityUid uid, double timeRemoved)
    {
        _statusEffectsSystem.TrySetTime(uid, StutterKey, TimeSpan.FromSeconds(timeRemoved));
    }

    public void DoRemoveStutter(EntityUid uid, double timeRemoved)
    {
       _statusEffectsSystem.TryRemoveStatusEffect(uid, StutterKey);
    }
}
