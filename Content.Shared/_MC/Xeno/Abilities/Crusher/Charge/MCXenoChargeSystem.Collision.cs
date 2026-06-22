using System.Numerics;
using Content.Shared._RMC14.Map;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

public sealed partial class MCXenoChargeSystem
{
    private void OnActiveToggleChargingCollide(Entity<MCXenoChargeActiveComponent> ent, ref PreventCollideEvent args)
    {
        if (float.Abs(ent.Comp.Steps - 1) < 0.001)
            return;

        if (args.OtherFixture.CollisionLayer == (int) CollisionGroup.SlipLayer)
            return;

        _hit.Add((ent, args.OtherEntity));

        if (HasComp<MobStateComponent>(args.OtherEntity))
            args.Cancelled = true;
    }

    private void OnDamageableHit(Entity<DamageableComponent> entity, ref MCXenoChargeCollideEvent args)
    {
        args.Handled = true;

        var chargerComp = args.Charger.Comp;
        var chargerProto = Comp<MCXenoChargeComponent>(args.Charger);

        if (chargerComp.Stage < chargerProto.MinimumSteps)
            return;

        var throwDirection = GetPerpendicularThrowDirection(chargerComp.Direction);

        if (Transform(entity).Anchored)
        {
            HandleStructureCollision(entity, args, chargerProto);
            return;
        }

        if (HasComp<MobStateComponent>(entity) && !_mobState.IsDead(entity))
            HandleMobCollision(entity, args, chargerProto, throwDirection);
    }

    private void HandleStructureCollision(Entity<DamageableComponent> entity, MCXenoChargeCollideEvent args, MCXenoChargeComponent charger)
    {
        if (_mcXenoHive.FromSameHive(entity.Owner, args.Charger.Owner))
            return;

        if (charger.StructureDamage is not null)
        {
            var damage = charger.StructureDamage * args.Charger.Comp.Stage * charger.SpeedPerStage;
            _damageable.TryChangeDamage(entity, damage);
        }

        IncrementStages(args.Charger, -2);
    }

    private void HandleMobCollision(Entity<DamageableComponent> entity, MCXenoChargeCollideEvent args, MCXenoChargeComponent charger, Vector2 throwDirection)
    {
        _throwing.TryThrow(entity, throwDirection, 5f);

        if (_mcXenoHive.FromSameHive(entity.Owner, args.Charger.Owner))
        {
            IncrementStages(args.Charger, -1);
            return;
        }

        if (charger.Damage is null)
            return;

        var damage = charger.Damage * args.Charger.Comp.Stage * charger.SpeedPerStage;
        _damageable.TryChangeDamage(entity, damage);
    }

    private Vector2 GetPerpendicularThrowDirection(DirectionFlag direction)
    {
        var perpendiculars = direction.AsDir().GetPerpendiculars();
        var perpendicular = _random.Prob(0.5f) ? perpendiculars.First : perpendiculars.Second;
        return perpendicular.ToVec().Normalized();
    }
}
