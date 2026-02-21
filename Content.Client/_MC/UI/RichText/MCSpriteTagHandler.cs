using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._MC.UI.RichText;

[UsedImplicitly]
public sealed class MCSpriteTagHandler : IMarkupTagHandler
{
    [Dependency] private readonly IEntityManager _entityManager = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    private static int _id;

    public string Name => "mcsprite";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;
        if (!node.Value.TryGetString(out var path))
            return false;

        var size = new Vector2(32);
        if (node.Attributes.TryGetValue("size", out var sizePara) && sizePara.TryGetLong(out var sizeValue))
            size = new Vector2((int) sizeValue);

        var spriteSystem = _entityManager.System<SpriteSystem>();
        control = new TextureRect
        {
            Name = $"__mcsprite_{_id++}",
            SetSize = size,
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
            Texture = spriteSystem.GetFrame(new SpriteSpecifier.Texture(new ResPath(path)), _timing.CurTime),
        };

        return true;
    }
}
