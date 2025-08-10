using Content.Server.GameTicking.Events;
using Content.Shared._RMC14.Marines.Squads;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Squad;

public sealed class MCSquadSystem : EntitySystem
{
    private static readonly List<EntProtoId> DefaultSquads =
    [
        "SquadAlpha", "SquadBravo", "SquadCharlie", "SquadDelta",
    ];

    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        SpawnSquads();
    }

    public (EntProtoId Id, EntityUid Ent) NextSquad(ProtoId<JobPrototype> job, EntProtoId<SquadTeamComponent>? preferred)
    {
        var data = GetData();
        var squads = new List<(EntProtoId SquadId, EntityUid Squad, int Players)>();

        foreach (var (squadId, squad) in data.Comp.Squads)
        {
            var players = 0;
            if (TryComp<SquadTeamComponent>(squad, out var team))
            {
                var roles = team.Roles;
                var maxRoles = team.MaxRoles;
                if (roles.TryGetValue(job, out var currentPlayers))
                    players = currentPlayers;

                if (preferred != null &&
                    preferred == squadId &&
                    (!maxRoles.TryGetValue(job, out var max) || players < max))
                {
                    return (squadId, squad);
                }
            }

            squads.Add((squadId, squad, players));
        }

        _random.Shuffle(squads);
        squads.Sort((a, b) => a.Players.CompareTo(b.Players));

        var chosen = squads[0];
        return (chosen.SquadId, chosen.Squad);
    }

    private void SpawnSquads()
    {
        var data = GetData();
        foreach (var id in DefaultSquads)
        {
            if (data.Comp.Squads.ContainsKey(id))
                continue;

            data.Comp.Squads[id] = Spawn(id);
            Log.Info($"Spawn squad: {id}");
        }
    }

    private Entity<MCSquadSystemDataComponent> GetData()
    {
        var query = EntityQueryEnumerator<MCSquadSystemDataComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            return (uid, component);
        }

        var instance = Spawn();
        return (instance, AddComp<MCSquadSystemDataComponent>(instance));
    }
}
