using Content.Shared._MC.Mob.Stamina;
using Content.Shared._MC.Stun;
using Content.Shared._RMC14.Deafness;
using Content.Shared.Coordinates;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Queen.Screech;

public sealed class MCXenoScreechSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDeafnessSystem _deaf = null!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;
    [Dependency] private readonly ExamineSystemShared _examineSystem = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCStunSystem _mcStun = null!;
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;

    private readonly HashSet<Entity<MobStateComponent>> _mobs = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoScreechComponent, MCXenoScreechActionEvent>(OnXenoScreechAction);
    }

    private void OnXenoScreechAction(Entity<MCXenoScreechComponent> entity, ref MCXenoScreechActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!TryUseAction(entity, args.Action))
            return;

        if (Net.IsServer)
        {
            _audio.PlayPvs(entity.Comp.SoundEffect, entity);
            SpawnAttachedTo(entity.Comp.EntProtoEffect, entity.Owner.ToCoordinates());
        }

        var xform = Transform(entity);

        _mobs.Clear();
        _entityLookup.GetEntitiesInRange(xform.Coordinates, entity.Comp.StunRange, _mobs);
        foreach (var receiver in _mobs)
        {
            if (receiver.Owner == entity.Owner)
                continue;

            if (!ValidateTarget(entity, receiver))
                continue;

            ApplyScreechEffects(entity, receiver);
        }
    }

    private void ApplyScreechEffects(Entity<MCXenoScreechComponent> entity, EntityUid target)
    {
        var distance = (_transform.GetWorldPosition(entity) - _transform.GetWorldPosition(target)).Length();
        var distPct = float.Clamp(distance / entity.Comp.StunRange, 0f, 1f);
        var canSee = _examineSystem.InRangeUnOccluded(entity, target);

        var reduction = canSee ? 1f : 0.2f;

        // TODO: protection_aura
        reduction = float.Clamp(reduction, 0.1f, 1.0f);

        var stunMultiplier = MathHelper.Lerp(1.0f, 0.4f, distPct) * reduction;
        var stunTime = TimeSpan.FromSeconds(2.0f * stunMultiplier);

        _deaf.TryDeafen(target, stunTime);

        _mcStun.Paralyze(target, stunTime);
        _mcStun.Stun(target, stunTime);

        _mcStamina.ApplyDamage(target, MathHelper.Lerp(140f, 70f, distPct) * reduction);
    }
}
