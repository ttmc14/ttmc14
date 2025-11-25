using System.Linq;
using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.Ui;
using Content.Shared._RMC14.Marines.Roles.Ranks;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSConsoleSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    [Dependency] private readonly SharedRankSystem _rmcRank = null!;

    [Dependency] private readonly MCASRSSystem _mcAsrs = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCASRSConsoleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleSendRequestMessage>(OnRequestMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleApproveMessage>(OnApproveMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleApproveAllMessage>(OnApproveAllMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleDenyMessage>(OnDenyMessage);
        SubscribeLocalEvent<MCASRSConsoleComponent, MCASRSConsoleDenyAllMessage>(OnDenyAllMessage);
    }

    private void OnInit(Entity<MCASRSConsoleComponent> entity, ref ComponentInit args)
    {
        entity.Comp.CachedEntries.Clear();
        foreach (var category in entity.Comp.Categories)
        {
            entity.Comp.CachedEntries.AddRange(category.Entries);
        }

        RefreshUI(entity);
    }

    private void OnRequestMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleSendRequestMessage args)
    {
        if (!ValidateRequestMessage(entity, args))
            return;

        var totalCost = 0;
        foreach (var (entry, count) in args.Contents)
        {
            totalCost += entry.Cost * count;
        }

        var name = GetRequesterName(args.Actor);
        var request = new MCASRSRequest(name, args.Reason, args.Contents, totalCost);

        entity.Comp.Requests.Add(request);

        Dirty(entity);
        RefreshUI(entity);
    }

    private void OnApproveAllMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleApproveAllMessage args)
    {
        var cost = entity.Comp.Requests.Sum(entry => entry.TotalCost);
        if (_mcAsrs.Points < cost)
            return;

        _mcAsrs.RemovePoints(cost);

        entity.Comp.ApprovedRequests.AddRange(entity.Comp.Requests);
        entity.Comp.Requests.Clear();
        Dirty(entity);
        RefreshUI(entity);
    }

    private void OnApproveMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleApproveMessage args)
    {
        if (!entity.Comp.Requests.Contains(args.Request))
            return;

        if (_mcAsrs.Points < args.Request.TotalCost)
            return;

        _mcAsrs.RemovePoints(args.Request.TotalCost);

        entity.Comp.Requests.Remove(args.Request);
        entity.Comp.ApprovedRequests.Add(args.Request);

        Dirty(entity);
        RefreshUI(entity);
    }

    private void OnDenyMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleDenyMessage args)
    {
        if (!entity.Comp.Requests.Contains(args.Request))
            return;

        entity.Comp.Requests.Remove(args.Request);
        entity.Comp.DenyRequests.Add(args.Request);

        Dirty(entity);
        RefreshUI(entity);
    }

    private void OnDenyAllMessage(Entity<MCASRSConsoleComponent> entity, ref MCASRSConsoleDenyAllMessage args)
    {
        entity.Comp.DenyRequests.AddRange(entity.Comp.Requests);
        entity.Comp.Requests.Clear();

        Dirty(entity);
        RefreshUI(entity);
    }

    private string GetRequesterName(EntityUid userUid)
    {
        return _rmcRank.GetSpeakerFullRankName(userUid) ?? Name(userUid);
    }

    private static bool ValidateRequestMessage(Entity<MCASRSConsoleComponent> entity, MCASRSConsoleSendRequestMessage args)
    {
        return args.Reason != string.Empty
               && args.Contents.Keys.All(entry => entity.Comp.CachedEntries.Contains(entry));
    }

    private void RefreshUI(Entity<MCASRSConsoleComponent> entity)
    {
        _userInterface.SetUiState(entity.Owner, MCASRSConsoleUi.Key, new MCASRSConsoleBuiState(_mcAsrs.Points, entity.Comp.Requests));
    }
}
