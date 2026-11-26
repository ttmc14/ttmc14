using Content.Shared._RMC14.Marines.Skills;
using Content.Shared._RMC14.TacticalMap;
using JetBrains.Annotations;

namespace Content.Client._RMC14.TacticalMap;

[UsedImplicitly]
public sealed class TacticalMapComputerBui(EntityUid owner, Enum uiKey) : TacticalMapBui(owner, uiKey)
{
    /// <inheritdoc/>
    public override void Refresh()
    {
        if (!EntMan.TryGetComponent<TacticalMapComputerComponent>(Owner, out var computer))
            return;

        var canDraw = PlayerManager.LocalEntity is { } player &&
            EntMan.System<SkillsSystem>().HasSkill(player, computer.Skill, computer.SkillLevel);
        UpdateControls(canDraw, computer.LastAnnounceAt, computer.NextAnnounceAt);
    }
}
