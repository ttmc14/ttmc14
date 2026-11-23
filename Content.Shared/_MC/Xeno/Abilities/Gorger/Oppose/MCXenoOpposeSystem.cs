using Content.Shared._MC.CameraShake;
using Content.Shared._MC.Damage;
using Content.Shared._MC.StatusEffects.SlowdownStacks;
using Content.Shared._MC.StatusEffects.Stun;
using Content.Shared._MC.Xeno.Abilities.Gorger.Oppose.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Oppose.Events;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared.Coordinates;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Oppose;

public sealed class MCXenoOpposeSystem : MCXenoAbilitySystem<MCXenoOpposeComponent, MCXenoOpposeActionEvent>
{
    [Dependency] private readonly MCCameraShakeSystem _shake = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly IMapManager _map = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCXenoOverhealSystem _overheal = null!;
    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly MCDamageableSystem _damage = null!;
    [Dependency] private readonly MCStunSystem _stun = null!;
    [Dependency] private readonly MCSlowdownStacksSystem _slowdown = null!;

    protected override void OnUse(Entity<MCXenoOpposeComponent> ent, ref MCXenoOpposeActionEvent args)
    {
        ServerSpawnAttachedTo("CMEffectSelfStomp", ent.Owner.ToCoordinates());

        var coords = Transform(ent).Coordinates;
        ServerSpawnAttachedTo("MCEffectOpposeShatter", coords);
        ServerSpawnAttachedTo("MCEffectOpposeParticles", coords);

        _audio.PlayPredicted(ent.Comp.StompSound, ent, ent);

        var center = coords.ToVector2i(EntityManager, _map, _transform);
        foreach (var mob in _lookup.GetEntitiesInRange<MobStateComponent>(coords, (ent.Comp.HealRange + 0.5f) * MathF.Sqrt(2), LookupFlags.Uncontained))
        {
            if (IsDead(mob.Owner))
                continue;

            var tile = Transform(mob).Coordinates.ToVector2i(EntityManager, _map, _transform);

            var distance = Math.Max(Math.Abs(tile.X - center.X), Math.Abs(tile.Y - center.Y));
            if (distance <= ent.Comp.HealRange && IsLivingHiveMember(ent, mob.Owner))
            {
                _overheal.Restore(mob.Owner, _heal.GetHealthAlive(mob.Owner) * ent.Comp.HealFraction, effect: "MCEffectBloodHealing");
                continue;
            }

            if (distance > 1)
                continue;

            if (distance == 0)
                _damage.AdjustBruteLoss(mob.Owner, ent.Comp.Damage);

            var strength = distance == 0 ? 3 : 2;

            _shake.ShakeCamera(mob.Owner, strength, strength);
            _stun.Stagger(mob.Owner, ent.Comp.StaggerDuration);
            _slowdown.AdjustSlowdown(mob.Owner, ent.Comp.Slowdown);
        }
    }
}
