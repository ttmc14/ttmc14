using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Bioscan;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public sealed class MCXenoHiveCollapseSystem : EntitySystem
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(10);

    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCXenoAnnounce _announce = null!;

    private TimeSpan _lastUpdate;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_lastUpdate > _timing.CurTime)
            return;

        _lastUpdate = UpdateInterval + _timing.CurTime;

        var query = EntityQueryEnumerator<MCXenoHiveComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var entity = (uid, component);
            if (!CanCollapse(entity))
                continue;

            ProcessCollapsing(entity);

            if (!IsCollapsing(entity))
                continue;

            ProcesseUncollapsing(entity);

            foreach (var (type, time) in component.Collapse)
            {
                if (time > _timing.CurTime)
                    continue;

                Collapse(entity, type);
                break;
            }
        }
    }

    private void ProcessCollapsing(Entity<MCXenoHiveComponent> entity)
    {
        if (!entity.Comp.Collapse.ContainsKey(MCXenoHiveCollapseType.Silo) && !_hive.HasSilo(entity))
        {
            StartCollapse(entity, MCXenoHiveCollapseType.Silo);
            AnnounceColapse(entity, MCXenoHiveCollapseType.Silo);
        }

        if (!entity.Comp.Collapse.ContainsKey(MCXenoHiveCollapseType.Ruler) && !_hive.HasRuler((entity, entity.Comp)))
        {
            StartCollapse(entity, MCXenoHiveCollapseType.Ruler);
            AnnounceColapse(entity, MCXenoHiveCollapseType.Ruler);
        }
    }

    private void ProcesseUncollapsing(Entity<MCXenoHiveComponent> entity)
    {
        if (entity.Comp.Collapse.ContainsKey(MCXenoHiveCollapseType.Silo) && _hive.HasSilo(entity))
        {
            StopCollapse(entity, MCXenoHiveCollapseType.Silo);
            return;
        }

        if (entity.Comp.Collapse.ContainsKey(MCXenoHiveCollapseType.Ruler) && _hive.HasRuler((entity, entity.Comp)))
        {
            StopCollapse(entity, MCXenoHiveCollapseType.Ruler);
            return;
        }
    }

    private void StartCollapse(Entity<MCXenoHiveComponent> entity, MCXenoHiveCollapseType type)
    {
        entity.Comp.Collapse[type] = entity.Comp.Configuration.General.CollapseTime[type] + _timing.CurTime;
        Dirty(entity);
    }

    private void StopCollapse(Entity<MCXenoHiveComponent> entity, MCXenoHiveCollapseType type)
    {
        entity.Comp.Collapse.Remove(type);
        Dirty(entity);
    }

    public bool IsCollapsing(Entity<MCXenoHiveComponent> entity)
    {
        return entity.Comp.Collapse.Count != 0;
    }

    public bool CanCollapse(Entity<MCXenoHiveComponent> entity)
    {
        return !entity.Comp.Collapsed && entity.Comp.Configuration.General.AllowCollapse;
    }

    public void Collapse(Entity<MCXenoHiveComponent> entity, MCXenoHiveCollapseType type)
    {
        entity.Comp.Collapsed = true;
        Dirty(entity);

        var ev = new MCXenoHiveCollapsed(entity, type);
        RaiseLocalEvent(ref ev);
    }

    #region Messages

    private void AnnounceColapse(Entity<MCXenoHiveComponent> entity, MCXenoHiveCollapseType type)
    {
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
        _announce.AnnounceToHive(entity, message, messageWraped, sound: sound);
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
        _announce.AnnounceToHive(entity, message, messageWraped, sound: sound);
    }

    #endregion
}
