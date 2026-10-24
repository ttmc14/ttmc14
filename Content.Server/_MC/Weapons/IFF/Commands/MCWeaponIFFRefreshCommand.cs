using Content.Server.Administration;
using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._MC.Weapons.IFF.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class MCWeaponIFFRefreshCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entity = null!;

    public override string Command => "mc_weapon_iff_refresh";
    public override string Help => "mc_weapon_iff_refresh <faction>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError($"Usage: ${Help}");
            return;
        }

        var query = _entity.EntityQueryEnumerator<MarineIFFComponent>();
        var system = _entity.System<GunIFFSystem>();

        while (query.MoveNext(out var uid, out _))
        {
            system.SetUserFaction(uid, args[0]);
        }
    }
}
