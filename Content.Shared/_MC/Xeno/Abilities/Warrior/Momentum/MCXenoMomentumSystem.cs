using System.Linq;
using Content.Shared._MC.Armor.Core.Events;
using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using JetBrains.Annotations;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.Momentum;

public sealed class MCXenoMomentumSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = null!;

    private EntityQuery<MCXenoMomentumComponent> _query;

    public override void Initialize()
    {
        _query = GetEntityQuery<MCXenoMomentumComponent>();

        SubscribeLocalEvent<MCXenoMomentumComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<MCXenoMomentumComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<MCXenoMomentumComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<MCXenoMomentumComponent, MCArmorGetEvent>(OnArmorGet);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCXenoMomentumComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Stacks == 0)
                continue;

            if (component.StacksDrainNext > _timing.CurTime)
                continue;

            OnStacksDrain((uid, component));
        }
    }

    private void OnStacksDrain(Entity<MCXenoMomentumComponent> entity)
    {
        SetStacks((entity.Owner, entity.Comp), 0);
    }

    #region Events gain

    private void OnMeleeHit(Entity<MCXenoMomentumComponent> entity, ref MeleeHitEvent args)
    {
        var count = args.HitEntities.Where(IsMob).Where(uid => !IsDead(uid)).Count(uid => !MCXenoHive.FromSameHive(entity.Owner, uid));
        if (count == 0)
            return;

        AddStacks((entity.Owner, entity.Comp), entity.Comp.StacksGainSlash * count);
    }

    #endregion

    #region Event bonus

    private void OnRefreshMovementSpeed(Entity<MCXenoMomentumComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (Net.IsClient)
            return;

        args.ModifySpeed(1 + entity.Comp.StacksSpeedBonus * entity.Comp.Stacks);
    }

    private void OnGetMeleeDamage(Entity<MCXenoMomentumComponent> entity, ref GetMeleeDamageEvent args)
    {
        if (Net.IsClient)
            return;

        args.Damage += new DamageSpecifier
        {
            DamageDict =
            {
                { "MCBrute", entity.Comp.StacksDamageBonus * entity.Comp.Stacks },
            },
        };
    }

    private void OnArmorGet(Entity<MCXenoMomentumComponent> entity, ref MCArmorGetEvent args)
    {
        if (Net.IsClient)
            return;

        args.SoftArmor += (int) (entity.Comp.StacksArmorBonus * entity.Comp.Stacks);
    }

    #endregion

    [PublicAPI]
    public void AddStacks(Entity<MCXenoMomentumComponent?> entity, int stacks, bool refreshDrain = true)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        SetStacks(entity, entity.Comp.Stacks + stacks);

        if (!refreshDrain)
            return;

        RefreshDrain(entity);
    }

    [PublicAPI]
    public bool TryRemoveStacks(Entity<MCXenoMomentumComponent?> entity, int stacks)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        if (entity.Comp.Stacks < stacks)
            return false;

        RemoveStacks(entity, stacks);
        return true;
    }

    [PublicAPI]
    public void RemoveStacks(Entity<MCXenoMomentumComponent?> entity, int stacks)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        SetStacks(entity, entity.Comp.Stacks - stacks);
    }

    [PublicAPI]
    public void SetStacks(Entity<MCXenoMomentumComponent?> entity, int stacks)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        if (entity.Comp.Stacks == stacks)
            return;

        entity.Comp.Stacks = int.Clamp(stacks, 0, entity.Comp.StacksMax);
        DirtyField(entity, entity.Comp, nameof(MCXenoMomentumComponent.Stacks));

        RefreshStacks(entity);
    }

    [PublicAPI]
    public void RefreshStacks(Entity<MCXenoMomentumComponent?> entity)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        _movementSpeed.RefreshMovementSpeedModifiers(entity);
    }

    [PublicAPI]
    public void RefreshDrain(Entity<MCXenoMomentumComponent?> entity)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        entity.Comp.StacksDrainNext = entity.Comp.StacksDrainDuration + _timing.CurTime;
        DirtyField(entity, entity.Comp, nameof(MCXenoMomentumComponent.StacksDrainNext));
    }
}
