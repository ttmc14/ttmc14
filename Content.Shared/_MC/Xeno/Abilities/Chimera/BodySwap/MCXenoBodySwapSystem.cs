using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Chimera.BodySwap;

public sealed class MCXenoBodySwapSystem : MCXenoAbilitySystem
{
    private static readonly LocId NotXenoLocId = "mc-xeno-ability-body-swap-not-xeno";
    private static readonly LocId OutOfRangeLocId = "mc-xeno-ability-body-swap-out-of-range";

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoBodySwapComponent, MCXenoBodySwapActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoBodySwapComponent> entity, ref MCXenoBodySwapActionEvent args)
    {
        if (args.Handled)
            return;

        if (!IsXeno(args.Target))
        {
            _popup.PopupClient(Loc.GetString(NotXenoLocId), entity, entity, PopupType.MediumCaution);
            return;
        }

        var delta = _transform.GetWorldPosition(entity) - _transform.GetWorldPosition(args.Target);
        if (delta.Length() > entity.Comp.Range)
        {
            _popup.PopupClient(Loc.GetString(OutOfRangeLocId), entity, entity, PopupType.MediumCaution);
            return;
        }

        if (!TryUseAction(entity, args.Action))
            return;

        var selfPosition = _transform.GetMapCoordinates(entity);
        var targetPosition = _transform.GetMapCoordinates(args.Target);

        _transform.SetMapCoordinates(entity, targetPosition);
        _transform.SetMapCoordinates(args.Target, selfPosition);

        _audio.PlayPredicted(entity.Comp.EffectSound, args.Target, entity);
        _audio.PlayPredicted(entity.Comp.EffectSound, entity, entity);

        args.Handled = true;
    }
}
