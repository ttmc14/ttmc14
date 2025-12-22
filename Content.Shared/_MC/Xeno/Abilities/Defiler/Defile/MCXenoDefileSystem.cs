using Content.Shared._MC.Damage;
using Content.Shared._MC.Spreader;
using Content.Shared._MC.Stamina;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.Defile;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoDefileSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = null!;
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;

    private readonly List<ReagentQuantity> _reagentIds = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoDefileComponent, MCXenoDefileActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoDefileComponent, MCXenoDefileDoAfterEvent>(OnActionDoAfter);
    }

    private void OnAction(Entity<MCXenoDefileComponent> entity, ref MCXenoDefileActionEvent args)
    {
        if (args.Handled || !CanUseAction(entity, args.Action, args.Target))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, new MCXenoDefileDoAfterEvent(args.Action, EntityManager), entity, args.Target)
        {
            DistanceThreshold = entity.Comp.Range,
            RequireCanInteract = false,
        });
    }

    private void OnActionDoAfter(Entity<MCXenoDefileComponent> entity, ref MCXenoDefileDoAfterEvent args)
    {
        var action = args.GetAction(EntityManager);
        if (args.Handled || args.Target is not {} target || !TryUseAction(entity, action, args.Target))
            return;

        args.Handled = true;

        if (args.Cancelled)
        {
            ActionSetUseDelay<MCXenoDefileActionEvent>(entity, action, entity.Comp.FailUseCooldown);
            return;
        }

        _mcStamina.Damage(target, 50, false);
        _mcDamageable.AdjustBruteLoss(target, 5);

        AnimateHit(entity, target);

        if (!_solutionContainer.TryGetSolution(target, entity.Comp.Solution, out var solutionEntity, out var solution))
            return;

        var power = 0f;
        foreach (var reagentQuantity in solution.Contents)
        {
            power += reagentQuantity.Quantity.Float();

            _reagentIds.Add(reagentQuantity);
        }

        foreach (var reagentId in _reagentIds)
        {
            _solutionContainer.RemoveReagent(solutionEntity.Value, reagentId);
        }

        _reagentIds.Clear();

        _mcDamageable.AdjustToxLoss(target, power);

        ActionStartUseDelay<MCXenoDefileActionEvent>(entity, action);

        var targetCoordinates = Transform(target).Coordinates;
        var smokeUid = ServerSpawn(entity.Comp.SmokeId, targetCoordinates);
        if (!smokeUid.Valid)
            return;

        _audio.PlayPvs(entity.Comp.SmokeEffectSound, targetCoordinates);

        RMCXenoHive.SetSameHive(entity.Owner, smokeUid);

        var spreader = EnsureComp<MCEdgeSpreaderComponent>(smokeUid);
        // TODO: [MC] smoke strength
        spreader.Range = int.Max((int) float.Round(power * 0.03f), 1);
        Dirty(smokeUid, spreader);
    }
}
