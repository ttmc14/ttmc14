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

        var size = 32;
        if (node.Attributes.TryGetValue("size", out var sizeParam) && sizeParam.TryGetLong(out var sizeValue))
            size = (int) sizeValue.Value;

        var spriteSystem = _entityManager.System<SpriteSystem>();

        control = new TextureRect
        {
            Name = $"__mcsprite_{_id++}",
            Texture = spriteSystem.GetFrame(new SpriteSpecifier.Texture(new ResPath(path)), _timing.CurTime),
            Stretch = TextureRect.StretchMode.Scale,
            HorizontalAlignment = Control.HAlignment.Center,
            VerticalAlignment = Control.VAlignment.Center,
            MinSize = new Vector2(size, size),
            CanShrink = true,
            Margin = new Thickness(1, 2, 1, 2),
        };

        return true;
    }
}
