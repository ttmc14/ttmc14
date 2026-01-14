using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;

public sealed class MCXenoGlobSystem : MCXenoAbilitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoGlobComponent, MCXenoCreateGlobActionEvent>(OnCreateAction);
    }

    private void OnCreateAction(Entity<MCXenoGlobComponent> entity, ref MCXenoCreateGlobActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        if (!AdjustValue((entity, entity), 1, args.Action))
            return;

        args.Handled = true;
    }

    public bool HasValue(Entity<MCXenoGlobComponent?> entity, int amount)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        return entity.Comp.Value >= amount;
    }

    public int GetValue(Entity<MCXenoGlobComponent?> entity)
    {
        return Resolve(entity, ref entity.Comp, logMissing: false) ? entity.Comp.Value : 0;
    }

    public bool AdjustValue(Entity<MCXenoGlobComponent?> entity, int amount, EntityUid? targetActionUid = null)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        var newValue = int.Clamp(entity.Comp.Value + amount, 0, entity.Comp.Max);
        if (newValue == entity.Comp.Value)
            return false;

        SetValue(entity, newValue, targetActionUid);
        return true;
    }

    public void SetValue(Entity<MCXenoGlobComponent?> entity, int value, EntityUid? targetActionUid = null)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        entity.Comp.Value = value;
        DirtyField(entity, nameof(MCXenoGlobComponent.Value));

        foreach (var actionEntity in RMCActions.GetActionsWithEvent<MCXenoCreateGlobActionEvent>(entity))
        {
            if (targetActionUid is not null && targetActionUid != actionEntity.Owner)
                continue;

            if (Actions.GetEvent(actionEntity) is not MCXenoCreateGlobActionEvent ev)
                continue;

            if (actionEntity.Comp.Icon is not SpriteSpecifier.Rsi icon)
                continue;

            Actions.SetIcon((actionEntity, actionEntity), new SpriteSpecifier.Rsi(icon.RsiPath, $"{ev.StatePrefix}{value}"));
        }
    }
}
