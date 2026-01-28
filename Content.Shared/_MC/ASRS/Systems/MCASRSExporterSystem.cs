using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.Chat;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSExporterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    [Dependency] private readonly MCASRSSystem _mcAsrs = null!;
    [Dependency] private readonly MCASRSCostSystem _mcAsrsCost = null!;
    [Dependency] private readonly MCSharedChatSystem _mcChat = null!;

    private readonly HashSet<EntityUid> _sellEntities = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCASRSExporterComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnInteractHand(Entity<MCASRSExporterComponent> entity, ref InteractHandEvent args)
    {
        if (!Transform(entity).Anchored)
        {
            _mcChat.TrySendInGameICSpeakMessage(entity, Loc.GetString("export-pad-not-anchored", ("pad", entity)));
            return;
        }

        if (entity.Comp.LastExportTime > _timing.CurTime)
        {
            var remaining = entity.Comp.LastExportTime - _timing.CurTime;
            var seconds = int.Max(1, (int) double.Ceiling(remaining.TotalSeconds));

            _mcChat.TrySendInGameICSpeakMessage(entity, Loc.GetString("export-pad-cooldown", ("pad", entity), ("seconds", seconds)));
            return;
        }

        Sell(entity);

        entity.Comp.LastExportTime = _timing.CurTime + entity.Comp.Cooldown;
        DirtyField(entity, entity.Comp, nameof(MCASRSExporterComponent.LastExportTime));
    }

    public void Sell(Entity<MCASRSExporterComponent> entity)
    {
        var announced = false;
        var value = 0;

        _sellEntities.Clear();
        _lookup.GetEntitiesIntersecting(entity, _sellEntities, LookupFlags.Dynamic | LookupFlags.Sundries);

        foreach (var uid in _sellEntities)
        {
            var cost = _mcAsrsCost.GetCost(uid);
            if (cost == 0)
            {
                Announce(Loc.GetString("export-pad-human-not-interesting"));
                continue;
            }

            if (_mobState.IsAlive(uid))
            {
                Announce(Loc.GetString("export-pad-human-alive"));
                continue;
            }

            value += cost;

            PredictedQueueDel(uid);
        }

        _mcAsrs.AddBalance(value);
        _mcChat.TrySendInGameICSpeakMessage(entity, Loc.GetString("export-pad-sold", ("pad", entity), ("points", value)));

        return;

        void Announce(string message)
        {
            if (announced)
                return;

            announced = true;
            _mcChat.TrySendInGameICSpeakMessage(entity, message);
        }
    }
}
