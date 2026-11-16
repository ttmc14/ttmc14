using Content.Shared._MC.Engineering.Sentries.Components;
using Content.Shared._MC.Engineering.Sentries.UI.Message;
using Content.Shared._RMC14.Interaction;

namespace Content.Shared._MC.Engineering.Sentries;

/// <summary>
/// Shared system handling the logic for switching between radial and directional sentry modes.
/// </summary>
public abstract class MCSentryRadialSharedSystem : EntitySystem
{
    [Dependency] private readonly MCSentryUISystem _mcSentryUi = null!;
    [Dependency] private readonly RMCInteractionSystem _rmcInteraction = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSentryRadialComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCSentryRadialComponent, MCSentryRadialModeBuiMessage>(OnRadialModeMessage);
    }

    private void OnStartup(Entity<MCSentryRadialComponent> entity, ref ComponentStartup args) =>
        Refresh(entity);

    private void OnRadialModeMessage(Entity<MCSentryRadialComponent> entity, ref MCSentryRadialModeBuiMessage args)
    {
        entity.Comp.RadialMode = args.Enabled;
        Dirty(entity);

        Refresh(entity);
    }

    /// <summary>
    /// Applies the current radial mode state to the entity's constraints and radius.
    /// </summary>
    private void Refresh(Entity<MCSentryRadialComponent> entity)
    {
        ApplyRotationConstraints(entity);
        ApplyRadius(entity);

        _mcSentryUi.Refresh(entity.Owner);
    }

    /// <summary>
    /// Removes rotation limits in radial mode, or applies directional limits otherwise.
    /// </summary>
    private void ApplyRotationConstraints(Entity<MCSentryRadialComponent> entity)
    {
        if (entity.Comp.RadialMode)
        {
            RemCompDeferred<MaxRotationComponent>(entity);
            return;
        }

        var currentDir = Transform(entity).LocalRotation.GetCardinalDir().ToAngle();
        _rmcInteraction.SetMaxRotation(entity.Owner, currentDir, entity.Comp.MaxDeviation);
    }

    /// <summary>
    /// Applies the correct radius based on the current mode.
    /// </summary>
    private void ApplyRadius(Entity<MCSentryRadialComponent> entity) =>
        SetRadius(entity, entity.Comp.RadialMode ? entity.Comp.RadiusReduced : entity.Comp.Radius);

    /// <summary>
    /// Updates the effective radius of the sentry.
    /// </summary>
    /// <param name="entity">The sentry entity.</param>
    /// <param name="radius">The new radius value to apply.</param>
    protected virtual void SetRadius(Entity<MCSentryRadialComponent> entity, float radius)
    {
    }
}
