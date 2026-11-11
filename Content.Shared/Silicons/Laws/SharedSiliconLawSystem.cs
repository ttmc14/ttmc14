using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Stunnable;
using Content.Shared.Wires;
using Robust.Shared.Audio;

namespace Content.Shared.Silicons.Laws;

/// <summary>
/// This handles getting and displaying the laws for silicons.
/// </summary>
public abstract partial class SharedSiliconLawSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        InitializeUpdater();
    }

    public virtual void NotifyLawsChanged(EntityUid uid, SoundSpecifier? cue = null)
    {

    }

    protected virtual void EnsureSubvertedSiliconRole(EntityUid mindId)
    {

    }

    protected virtual void RemoveSubvertedSiliconRole(EntityUid mindId)
    {

    }
}

[ByRefEvent]
public record struct SiliconEmaggedEvent(EntityUid user);
