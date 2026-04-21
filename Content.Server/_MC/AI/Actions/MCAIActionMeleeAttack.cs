using Content.Server.CombatMode;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Server.Weapons.Melee;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;

namespace Content.Server._MC.AI.Actions;

public sealed partial class MCAIActionMeleeAttack : MCAIAction<MCAIActionMeleeAttack>
{
    [DataField]
    public string TargetKey = string.Empty;

    [DataField]
    public float Range = 1.5f;

    [DataField]
    public float ReregisterThreshold = 1.5f;
}

public sealed partial class MCAIActionMeleeAttackSystem : MCAIActionSystem<MCAIActionMeleeAttack>
{
    [Dependency] private readonly NPCSteeringSystem _steering = null!;
    [Dependency] private readonly MeleeWeaponSystem _weapon = null!;
    [Dependency] private readonly TransformSystem _transform = null!;
    [Dependency] private readonly CombatModeSystem _combatMode = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    private EntityQuery<NPCSteeringComponent> _steeringQuery;

    public override void Initialize()
    {
        base.Initialize();

        _steeringQuery = GetEntityQuery<NPCSteeringComponent>();
    }

    protected override void OnActionStartup(Entity<MCAIAgentComponent> entity, ref MCAIActionStartupEvent<MCAIActionMeleeAttack> args)
    {
        _combatMode.SetInCombatMode(entity, true);

        if (!entity.Comp.Memory.ContainerTryGet<EntityUid>(args.Action.TargetKey, out var target))
            return;

        var targetCoordinates = Transform(target).Coordinates;

        var comp = _steering.Register(entity, targetCoordinates);
        comp.Range = args.Action.Range;
    }

    protected override MCAIActionStatus OnActionUpdate(Entity<MCAIAgentComponent> entity, MCAIActionMeleeAttack action, float frameTime)
    {
        if (!entity.Comp.Memory.ContainerTryGet<EntityUid>(action.TargetKey, out var targetUid))
            return MCAIActionStatus.Failed;

        if (_mobState.IsDead(targetUid))
            return MCAIActionStatus.Finished;

        if (!_weapon.TryGetWeapon(entity, out var weaponUid, out var meleeWeaponComponent))
            return MCAIActionStatus.Failed;

        var targetCoordinates = Transform(targetUid).Coordinates;
        var delta = _transform.GetWorldPosition(entity) - _transform.GetWorldPosition(targetUid);

        if (_steeringQuery.TryComp(entity, out var steering))
        {
            if (delta.LengthSquared() > action.ReregisterThreshold * action.ReregisterThreshold)
            {
                var comp = _steering.Register(entity, targetCoordinates);
                comp.Range = action.Range;
            }

            if (steering.Status == SteeringStatus.NoPath)
                return MCAIActionStatus.Failed;
        }

        if (delta.LengthSquared() <= action.Range * action.Range)
            _weapon.AttemptLightAttack(entity, weaponUid, meleeWeaponComponent, targetUid);

        return MCAIActionStatus.Running;
    }

    protected override void OnActionShutdown(Entity<MCAIAgentComponent> entity, ref MCAIActionShutdownEvent<MCAIActionMeleeAttack> args)
    {
        _combatMode.SetInCombatMode(entity, false);
        _steering.Unregister(entity);
    }
}
