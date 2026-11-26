using System.Linq;
using Content.Shared._MC.StatusEffects.Stun;
using Content.Shared._MC.Xeno.Abilities.Warrior.Agility;
using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;

namespace Content.IntegrationTests.Tests;

public sealed class MCStaggerTest
{
    [Test]
    public async Task ReapplicationPreservesOriginalExpiryAndExpiryRestoresActions()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Map = "MCTalos", NoLoadTestPrototypes = true, Connected = true });
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var (server, client) = pair;
        var session = server.PlayerMan.GetSessionById(client.Session!.UserId);
        var previous = session.AttachedEntity;
        var clientGuns = client.System<SharedGunSystem>();
        var console = server.ResolveDependency<IConsoleHost>();
        var entities = server.EntMan;
        var stun = server.System<MCStunSystem>();
        var effects = server.System<SharedStatusEffectsSystem>();
        EntityUid target = default;
        EntityUid punch = default;

        var actions = server.System<SharedActionsSystem>();
        var rmcActions = server.System<SharedRMCActionsSystem>();
        var spits = server.System<MCSharedXenoSpitSystem>();
        var guns = server.System<SharedGunSystem>();
        Entity<ActionComponent> agility = default;
        Entity<GunComponent> gun = default;

        var aim = new EntityCoordinates(map.Grid, 6, 0);

        float ShootSpit(float damageMultiplier = 1f)
        {
            var projectile = spits.Shoot(target, aim, "MCXenoSpitSpitter", 1, Angle.Zero).Single();
            Assert.That(entities.GetComponent<ProjectileComponent>(projectile).ShotDamageMultiplier, Is.EqualTo(damageMultiplier));
            return entities.GetComponent<PhysicsComponent>(projectile).LinearVelocity.Y;
        }

        float ShootRifle(float damageMultiplier = 1f)
        {
            var bullet = entities.SpawnEntity("MCBulletRifleBR8", map.GridCoords);
            guns.Shoot(gun.Owner, gun.Comp, bullet, map.GridCoords, aim, out _, target);
            Assert.That(entities.GetComponent<ProjectileComponent>(bullet).ShotDamageMultiplier, Is.EqualTo(damageMultiplier));
            return entities.GetComponent<PhysicsComponent>(bullet).LinearVelocity.Y;
        }

        bool IsBlocked(EntityUid action)
        {
            var attempt = new ActionAttemptEvent(target);
            entities.EventBus.RaiseLocalEvent(action, ref attempt);
            return attempt.Cancelled;
        }

        await server.WaitAssertion(() =>
        {
            target = entities.SpawnEntity("MCXenoWarrior", map.GridCoords);
            server.PlayerMan.SetAttachedEntity(session, target);
            var rifle = entities.SpawnEntity("MCWeaponRifleBR8", map.GridCoords);
            gun = (rifle, entities.GetComponent<GunComponent>(rifle));
            punch = entities.SpawnEntity("MCActionXenoPunch", map.GridCoords);
            var rest = entities.SpawnEntity("MCActionXenoRest", map.GridCoords);
            Assert.That(IsBlocked(punch), Is.False);

            agility = rmcActions.GetActionsWithEvent<MCXenoAgilityActionEvent>(target).Single();
            actions.PerformAction(target, agility);
            Assert.That(entities.HasComponent<MCXenoAgilityActiveComponent>(target), Is.True);
            Assert.That(ShootSpit(), Is.Zero.Within(0.00001));
            var rifleVelocity = ShootRifle();

            Assert.That(entities.HasComponent<StatusEffectContainerComponent>(target), Is.False);
            console.ExecuteCommand($"ent {target} mc_status:set MCStatusEffectStagger 1");
            Assert.That(entities.HasComponent<StatusEffectContainerComponent>(target), Is.True);
            Assert.That(effects.TryGetStatusEffect(target, "MCStatusEffectStagger", out var effect), Is.True);
            var status = entities.GetComponent<StatusEffectComponent>(effect!.Value);
            var initialEnd = status.EndEffectTime;
            console.ExecuteCommand($"ent {target} rep 1 mc_status:add MCStatusEffectStagger 0.5");
            Assert.That(status.EndEffectTime, Is.EqualTo(initialEnd + TimeSpan.FromSeconds(0.5)));
            console.ExecuteCommand($"ent {target} rep 1 mc_status:set MCStatusEffectStagger 1");
            Assert.That(status.EndEffectTime, Is.EqualTo(initialEnd));
            Assert.That(IsBlocked(punch), Is.True);
            Assert.That(IsBlocked(rest), Is.False);
            Assert.That(agility.Comp.Enabled, Is.False);
            Assert.That(ShootSpit(0.5f), Is.Not.Zero);
            Assert.That(ShootRifle(0.5f), Is.Not.EqualTo(rifleVelocity));

            stun.Stagger(target, TimeSpan.FromMilliseconds(100));
            Assert.That(status.EndEffectTime, Is.EqualTo(initialEnd));
            stun.Stagger(target, TimeSpan.FromSeconds(2));
            Assert.That(status.EndEffectTime, Is.EqualTo(initialEnd));
        });

        await pair.RunTicksSync(5);
        var clientXeno = pair.ToClientUid(target);
        var clientAction = pair.ToClientUid(agility.Owner);
        await client.WaitAssertion(() =>
        {
            Assert.That(client.EntMan.GetComponent<ActionComponent>(clientAction).Enabled, Is.False);
            Assert.That(clientGuns.GetShooterSpread(clientXeno).Degrees, Is.EqualTo(10).Within(0.001));
        });

        await pair.RunTicksSync(pair.SecondsToTicks(1) + 1);
        await server.WaitAssertion(() =>
        {
            Assert.That(effects.HasStatusEffect(target, "MCStatusEffectStagger"), Is.False);
            Assert.That(IsBlocked(punch), Is.False);
            actions.PerformAction(target, agility);
            Assert.That(entities.HasComponent<MCXenoAgilityActiveComponent>(target), Is.False);
            Assert.That(ShootSpit(), Is.Zero.Within(0.00001));
            ShootRifle();
        });

        await client.WaitAssertion(() =>
        {
            Assert.That(client.EntMan.GetComponent<ActionComponent>(clientAction).Enabled, Is.True);
            Assert.That(clientGuns.GetShooterSpread(clientXeno), Is.EqualTo(Angle.Zero));
        });
        await server.WaitAssertion(() =>
        {
            stun.Stagger(target, TimeSpan.FromSeconds(2));
            Assert.That(IsBlocked(punch), Is.True);
            server.PlayerMan.SetAttachedEntity(session, previous);
            server.System<SharedMapSystem>().DeleteMap(map.MapId);
        });
        await pair.CleanReturnAsync();
    }
}
