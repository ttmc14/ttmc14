using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._RMC14.CCVar;
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.TacticalMap;

public abstract class SharedTacticalMapSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public int LineLimit { get; private set; }

    public override void Initialize()
    {
        SubscribeLocalEvent<TacticalMapUserComponent, OpenTacticalMapActionEvent>(OnUserOpenAction);
        SubscribeLocalEvent<TacticalMapUserComponent, OpenTacMapAlertEvent>(OnUserOpenAlert);

        Subs.CVar(_config, RMCCVars.RMCTacticalMapLineLimit, v => LineLimit = v, true);
    }

    private void OnUserOpenAction(Entity<TacticalMapUserComponent> ent, ref OpenTacticalMapActionEvent args)
    {
        ToggleMapUI(ent);
    }

    private void OnUserOpenAlert(Entity<TacticalMapUserComponent> ent, ref OpenTacMapAlertEvent args)
    {
        ToggleMapUI(ent);
    }

    public bool TryGetTacticalMap(out Entity<TacticalMapComponent> map)
    {
        var query = EntityQueryEnumerator<TacticalMapComponent>();
        while (query.MoveNext(out var uid, out var mapComp))
        {
            if (TryComp<CEZLevelMapComponent>(uid, out var level) && level.Depth != 0)
                continue;

            map = (uid, mapComp);
            return true;
        }

        map = default;
        return false;
    }

    protected void UpdateMapData(Entity<TacticalMapComputerComponent> computer)
    {
        if (!TryGetTacticalMap(out var map))
            return;

        UpdateMapData(computer, map);
    }

    protected void UpdateMapData(Entity<TacticalMapComputerComponent> computer, TacticalMapComponent map)
    {
        computer.Comp.Blips = GetComputerBlips(map);
        Dirty(computer);

        var lines = EnsureComp<TacticalMapLinesComponent>(computer);
        lines.MarineLines = map.MarineLines;
        Dirty(computer, lines);
    }

    protected Dictionary<int, TacticalMapBlip> GetComputerBlips(TacticalMapComponent map)
    {
        var ev = new TacticalMapIncludeXenosEvent();
        RaiseLocalEvent(ref ev);
        if (!ev.Include)
            return map.MarineBlips;

        var blips = new Dictionary<int, TacticalMapBlip>(map.MarineBlips);
        foreach (var (id, blip) in map.XenoBlips)
        {
            blips.TryAdd(id, blip);
        }

        return blips;
    }

    public void OpenComputerMap(Entity<TacticalMapComputerComponent?> computer, EntityUid user)
    {
        if (!Resolve(computer, ref computer.Comp, false))
            return;

        _ui.TryOpenUi(computer.Owner, TacticalMapComputerUi.Key, user);
        UpdateMapData((computer, computer.Comp));
    }

    public virtual void UpdateUserData(Entity<TacticalMapUserComponent> user, TacticalMapComponent map)
    {
    }

    private void ToggleMapUI(Entity<TacticalMapUserComponent> user)
    {
        if (_ui.IsUiOpen(user.Owner, TacticalMapUserUi.Key, user))
        {
            _ui.CloseUi(user.Owner, TacticalMapUserUi.Key, user);
            return;
        }

        _ui.TryOpenUi(user.Owner, TacticalMapUserUi.Key, user);
    }
}
