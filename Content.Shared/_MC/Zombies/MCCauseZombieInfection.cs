using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Zombies;

public sealed partial class MCCauseZombieInfection : EventEntityEffect<MCCauseZombieInfection>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-cause-mc-zombie-infection", ("chance", Probability));
}
