using Content.Shared._MC.Engineering.Sentries.Components;
using Content.Shared._MC.Engineering.Deploy.Components;
using Content.Shared._MC.Engineering.Deploy.Events;
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
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSentryRadialComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MCSentryRadialComponent, MCDeployChangedStateEvent>(OnDeployChanged);
        SubscribeLocalEvent<MCSentryRadialComponent, MCSentryRadialModeBuiMessage>(OnRadialModeMessage);
    }

    private void OnMapInit(Entity<MCSentryRadialComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.DeploymentRotation ??= Transform(entity).LocalRotation;
        Dirty(entity);
        Refresh(entity);
    }

    private void OnDeployChanged(Entity<MCSentryRadialComponent> entity, ref MCDeployChangedStateEvent args)
    {
        if (args.State != MCDeployState.Deployed)
            return;

        entity.Comp.DeploymentRotation = Transform(entity).LocalRotation;
        Dirty(entity);
        ApplyRotationConstraints(entity);
    }

    private void OnRadialModeMessage(Entity<MCSentryRadialComponent> entity, ref MCSentryRadialModeBuiMessage args)
    {
        if (entity.Comp.RadialMode == args.Enabled)
            return;

        entity.Comp.RadialMode = args.Enabled;
        Dirty(entity);

        if (!args.Enabled && entity.Comp.DeploymentRotation is { } rotation)
        {
            _transform.SetLocalRotation(entity.Owner, rotation);
        }

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
            RemComp<MaxRotationComponent>(entity);
            return;
        }

        var rotation = entity.Comp.DeploymentRotation;
        if (rotation is null)
            return;

        _rmcInteraction.SetMaxRotation(entity.Owner, rotation.Value, entity.Comp.MaxDeviation);
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
