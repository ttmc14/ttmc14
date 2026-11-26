using Content.Shared._MC.Rules.Warfare;
using Content.Shared._MC.Xeno.Hive.Announce;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Bioscan;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Hive.Collapse;

public sealed class MCXenoHiveCollapseSystem : EntitySystem
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(10);

    [Dependency] private readonly MCWarfareSystem _warfare = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCXenoAnnounceSystem _announceSystem = null!;

    private TimeSpan _lastUpdate;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient || _lastUpdate > _timing.CurTime)
            return;

        _lastUpdate = UpdateInterval + _timing.CurTime;

        if (_warfare.ActiveRule is { Landed: false })
            return;

        var query = EntityQueryEnumerator<MCXenoHiveComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var entity = (uid, component);
            if (!CanCollapse(entity))
                continue;

            UpdateCollapse(entity);
        }
    }

    private void UpdateCollapse(Entity<MCXenoHiveComponent> entity)
    {
        foreach (var (type, delay) in entity.Comp.Configuration.General.CollapseTime)
        {
            var recovered = type switch
            {
                MCXenoHiveCollapseType.Silo => _hive.HasSilo(entity),
                MCXenoHiveCollapseType.Ruler => _hive.HasRuler((entity, entity.Comp)),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };

            if (recovered)
            {
                if (entity.Comp.Collapse.Remove(type))
                {
                    Dirty(entity);
                }
                continue;
            }

            if (!entity.Comp.Collapse.TryGetValue(type, out var deadline))
            {
                entity.Comp.Collapse[type] = _timing.CurTime + delay;
                Dirty(entity);
                AnnounceCollapse(entity, type, delay);
                continue;
            }

            if (_timing.CurTime < deadline)
                continue;

            Collapse(entity, type);
            return;
        }
    }

    public bool IsCollapsing(Entity<MCXenoHiveComponent> entity)
    {
        return entity.Comp.Collapse.Count != 0;
    }

    public bool CanCollapse(Entity<MCXenoHiveComponent> entity)
    {
        return entity.Comp is { Collapsed: false, Configuration.General.AllowCollapse: true };
    }

    public void Collapse(Entity<MCXenoHiveComponent> entity, MCXenoHiveCollapseType type)
    {
        entity.Comp.Collapsed = true;
        Dirty(entity);

        var ev = new MCXenoHiveCollapsed(entity, type);
        RaiseLocalEvent(ref ev);
    }

    #region Messages

    private void AnnounceCollapse(Entity<MCXenoHiveComponent> entity, MCXenoHiveCollapseType type, TimeSpan delay)
    {
        if (type == MCXenoHiveCollapseType.Silo && _warfare.ActiveRule is not null)
        {
            var message = Loc.GetString("mc-warfare-silo-warning", ("minutes", delay.TotalMinutes));
            _announceSystem.AnnounceToHive(entity, message, $"[bold]{message}[/bold]", sound: new BioscanComponent().XenoSound);
            return;
        }

        switch (type)
        {
            case MCXenoHiveCollapseType.Silo:
                AnnounceSiloColapse(entity);
                break;

            case MCXenoHiveCollapseType.Ruler:
                AnnounceRulerColapse(entity);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void AnnounceSiloColapse(Entity<MCXenoHiveComponent> entity)
    {

        const string message = """
            Проводник уничтожен! Связь с центром цепи распадается!
            У вас [color=#e0102f][font size=22]5 минут[/font][/color].
            Если канал не восстановить — порядок умрёт, и Улей погрузится в хаос.
            Время идёт… и оно не ждёт.
            """;

        const string messageWraped = $"""

            [bold][color=#e0102f][font size=24]Директива ИЗНАЧАЛЬНОГО РАЗУМА УЛЬЯ[/font][/color][/bold]


            [color=#850c1e][font size=16][bold]{message}[/bold][/font][/color]
            """;

        var sound = new BioscanComponent().XenoSound;
        _announceSystem.AnnounceToHive(entity, message, messageWraped, sound: sound);
    }

    private void AnnounceRulerColapse(Entity<MCXenoHiveComponent> entity)
    {
        const string message = """
            Глава улья пал. Его разум погас!
            У вас [color=#e0102f][font size=22]5 минут[/font][/color].
            Если глава не восстанет — моих сил не хватит на управление, и Улей погрузится в хаос.
            Время идёт… и оно не ждёт.
            """;

        const string messageWraped = $"""

            [bold][color=#e0102f][font size=24]Директива ИЗНАЧАЛЬНОГО РАЗУМА УЛЬЯ[/font][/color][/bold]


            [color=#850c1e][font size=16][bold]{message}[/bold][/font][/color]
            """;

        var sound = new BioscanComponent().XenoSound;
        _announceSystem.AnnounceToHive(entity, message, messageWraped, sound: sound);
    }

    #endregion
}
