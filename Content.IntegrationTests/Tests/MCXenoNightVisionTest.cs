using Content.Shared._RMC14.NightVision;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCXenoNightVisionTest
{
    [Test]
    public async Task HalfVisionSurvivesEyeStateUpdate()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = true,
            NoLoadTestPrototypes = true,
            Map = "MCTalos",
        });
        var (server, client) = pair;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var session = server.PlayerMan.GetSessionById(client.Session!.UserId);
        var previous = session.AttachedEntity;
        var vision = server.System<SharedNightVisionSystem>();
        EntityUid xeno = default;

        await server.WaitPost(() =>
        {
            xeno = server.EntMan.SpawnEntity("MCXenoRunner", map.GridCoords);
            server.PlayerMan.SetAttachedEntity(session, xeno);
            vision.SetState(xeno, NightVisionState.Half);
        });
        await pair.RunTicksSync(5);
        var clientXeno = pair.ToClientUid(xeno);

        await server.WaitPost(() =>
        {
            var eye = server.EntMan.GetComponent<EyeComponent>(xeno);
            server.EntMan.DirtyField(xeno, eye, nameof(EyeComponent.DrawFov));
        });
        await pair.RunTicksSync(5);
        await client.WaitAssertion(() =>
            Assert.That(client.EntMan.GetComponent<EyeComponent>(clientXeno).Eye.DrawFov, Is.False));

        await server.WaitPost(() => vision.SetState(xeno, NightVisionState.Off));
        await pair.RunTicksSync(5);
        await client.WaitAssertion(() =>
            Assert.That(client.EntMan.GetComponent<EyeComponent>(clientXeno).Eye.DrawFov, Is.True));

        await server.WaitPost(() =>
        {
            server.PlayerMan.SetAttachedEntity(session, previous);
            server.System<SharedMapSystem>().DeleteMap(map.MapId);
        });
        await pair.CleanReturnAsync();
    }
}
