using System.Diagnostics.CodeAnalysis;
using Content.Server.Fax;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC;
using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._RMC14.Bioscan;
using Content.Shared._RMC14.Thunderdome;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Fax.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._MC.Rules;

public abstract class MCRuleSystem<T> : GameRuleSystem<T> where T : IComponent
{
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly MapSystem _mapSystem = null!;
    [Dependency] private readonly IConfigurationManager _config = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly FaxSystem _fax = null!;
    [Dependency] private readonly GunIFFSystem _gunIFF = null!;

    protected bool RoundCheckEnding;
    protected float MarinesPerXeno;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, MCConfigVars.RoundCanEnd, v => RoundCheckEnding = v, true);
        Subs.CVar(_config, MCConfigVars.BalanceXenoRatio, v => MarinesPerXeno = v, true);
    }

    protected int GetXenos(int players)
    {
        return (int) Math.Round(Math.Max(1, players / MarinesPerXeno));
    }

    protected void SpawnAdminAreas(ResPath thunderdome)
    {
        if (SpawnMap(thunderdome, out var mapEnt))
            EnsureComp<ThunderdomeMapComponent>(mapEnt.Value);

        return;

        bool SpawnMap(ResPath path, [NotNullWhen(true)] out EntityUid? mapEntity)
        {
            mapEntity = null;

            try
            {
                if (string.IsNullOrWhiteSpace(path.ToString()))
                    return false;

                if (!_mapLoader.TryLoadMap(path, out var map, out _))
                    return false;

                _mapSystem.InitializeMap((map.Value, map.Value));
                mapEntity = map;
                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"Error loading admin fax area:\n{exception}");
            }

            return false;
        }
    }

    protected void StartBioscan()
    {
        EnsureComp<BioscanComponent>(Spawn(null, MapCoordinates.Nullspace));
    }

    protected void RefreshIFF(EntProtoId<IFFFactionComponent> faction)
    {
        var marineFactions = EntityQueryEnumerator<MarineIFFComponent>();
        while (marineFactions.MoveNext(out var iffId, out _))
        {
            _gunIFF.SetUserFaction(iffId, faction);
        }
    }

    protected void RefreshFaxes()
    {
        var faxes = EntityQueryEnumerator<FaxMachineComponent>();
        while (faxes.MoveNext(out var faxId, out var faxComp))
        {
            _fax.Refresh(faxId, faxComp);
        }
    }

    protected void SpawnNukeDiskGenerators()
    {
        var protoIds = new EntProtoId[]
        {
            "MCComputerNukeDiskGeneratorRed",
            "MCComputerNukeDiskGeneratorGreen",
            "MCComputerNukeDiskGeneratorBlue",
        };

        var coordinates = new List<MapCoordinates>();
        var query = EntityQueryEnumerator<MCNukeDiskGeneratorSpawnerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            coordinates.Add(_transform.GetMapCoordinates(uid));
        }

        foreach (var protoId in protoIds)
        {
            if (coordinates.Count == 0)
            {
                Log.Error($"Failed to spawn {protoId}, no available coordinates. Ensure that MCNukeDiskGeneratorSpawnerComponent exists on the map.");
                break;
            }

            Spawn(protoId, _random.PickAndTake(coordinates));
        }
    }
}
