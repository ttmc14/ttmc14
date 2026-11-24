using System.Linq;
using Content.Server.Administration;
using Content.Server.StatusEffectNew;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Server._MC.StatusEffects.Commands;

/// <summary>
/// Adjusts timed status effects on entities selected in Toolshed.
/// </summary>
[ToolshedCommand(Name = "mc_status"), AdminCommand(AdminFlags.Debug)]
public sealed class MCStatusCommand : ToolshedCommand
{
    /// <summary>
    /// Adds seconds to an effect. A positive duration also applies a missing effect.
    /// </summary>
    [CommandImplementation("add")]
    public EntityUid Add([PipedArgument] EntityUid target, EntProtoId effect, double seconds)
    {
        var effects = GetSys<StatusEffectsSystem>();
        var duration = TimeSpan.FromSeconds(seconds);
        if (duration > TimeSpan.Zero)
        {
            effects.TryAddStatusEffectDuration(target, effect, duration);
            return target;
        }

        effects.TryAddTime(target, effect, duration);
        return target;
    }

    /// <summary>
    /// Adds the duration to the effect on each selected entity.
    /// </summary>
    [CommandImplementation("add")]
    public IEnumerable<EntityUid> Add([PipedArgument] IEnumerable<EntityUid> targets, EntProtoId effect, double seconds)
        => targets.Select(target => Add(target, effect, seconds)).ToArray();

    /// <summary>
    /// Applies the effect or replaces its remaining duration in seconds.
    /// </summary>
    [CommandImplementation("set")]
    public EntityUid Set([PipedArgument] EntityUid target, EntProtoId effect, double seconds)
    {
        GetSys<StatusEffectsSystem>().TrySetStatusEffectDuration(target, effect, TimeSpan.FromSeconds(seconds));
        return target;
    }

    /// <summary>
    /// Applies the effect or replaces its remaining duration on each selected entity.
    /// </summary>
    [CommandImplementation("set")]
    public IEnumerable<EntityUid> Set([PipedArgument] IEnumerable<EntityUid> targets, EntProtoId effect, double seconds)
        => targets.Select(target => Set(target, effect, seconds)).ToArray();
}
