using Content.Shared._MC.StatusEffects.SlowdownStacks;
using Content.Shared.Movement.Components;
using Content.Shared.StatusEffectNew;

namespace Content.IntegrationTests.Tests.Movement;

public sealed class MCSlowdownStacksTest : MovementTest
{
    [Test]
    public async Task SlowdownDecaysAndExpires()
    {
        var slowdown = Server.System<MCSlowdownStacksSystem>();
        var effects = Server.System<SharedStatusEffectsSystem>();
        var movement = Comp<MovementSpeedModifierComponent>(Player);
        var initialSpeed = movement.SprintSpeedModifier;
        await Server.WaitAssertion(() =>
        {
            slowdown.AdjustSlowdown(SPlayer, 0.3f);
            Assert.That(movement.SprintSpeedModifier, Is.EqualTo(initialSpeed * MathF.Exp(-0.03f)).Within(0.0001f));
        });

        await Pair.RunSeconds(0.5f);
        await Server.WaitAssertion(() =>
        {
            Assert.That(effects.HasStatusEffect(SPlayer, "MCStatusEffectSlowdownStacks"), Is.True);
            Assert.That(movement.SprintSpeedModifier, Is.InRange(initialSpeed * 0.975f, initialSpeed * 0.995f));
        });

        await Pair.RunSeconds(1f);
        await Server.WaitAssertion(() =>
        {
            Assert.That(effects.HasStatusEffect(SPlayer, "MCStatusEffectSlowdownStacks"), Is.False);
            Assert.That(movement.SprintSpeedModifier, Is.EqualTo(initialSpeed).Within(0.0001f));
        });
    }
}
