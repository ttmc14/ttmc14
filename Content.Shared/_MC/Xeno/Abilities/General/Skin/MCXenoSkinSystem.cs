using Content.Shared._MC.Sprite;
using Content.Shared._MC.Xeno.Abilities.General.Skin.UI;

namespace Content.Shared._MC.Xeno.Abilities.General.Skin;

public sealed class MCXenoSkinSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;
    [Dependency] private readonly MCSpriteSystem _mcSprite = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoSkinComponent, MCXenoSkinActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoSkinComponent, MCXenoSkinSelectBuiMessage>(OnSelectUIMessage);
        SubscribeLocalEvent<MCXenoSkinComponent, MCXenoSkinResetBuiMessage>(OnResetUIMessage);
    }

    private void OnAction(Entity<MCXenoSkinComponent> entity, ref MCXenoSkinActionEvent args)
    {
        if (args.Handled)
            return;

        _userInterface.TryOpenUi(entity.Owner, MCXenoSkinUI.Key, entity, predicted: true);
        args.Handled = true;
    }

    private void OnSelectUIMessage(Entity<MCXenoSkinComponent> entity, ref MCXenoSkinSelectBuiMessage args)
    {
        _userInterface.CloseUi(entity.Owner, MCXenoSkinUI.Key, entity, predicted: true);

        if (!entity.Comp.Skins.TryGetValue(args.State, out var skinPath))
            return;

        _mcSprite.Change(entity.Owner, skinPath);
    }

    private void OnResetUIMessage(Entity<MCXenoSkinComponent> entity, ref MCXenoSkinResetBuiMessage args)
    {
        _userInterface.CloseUi(entity.Owner, MCXenoSkinUI.Key, entity, predicted: true);
        _mcSprite.Reset(entity.Owner);
    }
}
