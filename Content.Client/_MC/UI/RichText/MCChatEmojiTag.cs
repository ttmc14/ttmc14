using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client._MC.UI.RichText;

[UsedImplicitly]
public sealed class MCChatEmojiTag : IMarkupTagHandler
{
    public const string TagName = "mcchatemoji";

    [Dependency] private readonly IResourceCache _resources = default!;

    public string Name => TagName;

    // One object replacement character gives the image a position in the word-wrap pass.
    public string TextBefore(MarkupNode node) => "\uFFFC";

    public void PushDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        var font = context.Font.Peek();
        if (node.Value.StringValue is { } path)
        {
            var texture = _resources.GetResource<TextureResource>(new ResPath(path)).Texture;
            font = new EmojiFont(font, texture);
        }

        context.Font.Push(font);
    }

    public void PopDrawContext(MarkupNode node, MarkupDrawingContext context) => context.Font.Pop();

    private sealed class EmojiFont(Font textFont, Texture texture) : Font
    {
        public override int GetAscent(float scale) => textFont.GetAscent(scale);
        public override int GetDescent(float scale) => textFont.GetDescent(scale);
        public override int GetHeight(float scale) => textFont.GetHeight(scale);
        public override int GetLineHeight(float scale) => textFont.GetLineHeight(scale);

        public override CharMetrics? GetCharMetrics(Rune rune, float scale, bool fallback = true)
        {
            var size = GetHeight(scale);
            return new CharMetrics(0, GetAscent(scale), size, size, size);
        }

        public override float DrawChar(DrawingHandleBase handle, Rune rune, Vector2 baseline, float scale,
            Color color, bool fallback = true)
        {
            var size = GetHeight(scale);
            var imageSize = (Vector2) texture.Size * ((float) size / Math.Max(texture.Width, texture.Height));
            var topLeft = baseline - new Vector2(0, GetAscent(scale)) + (new Vector2(size) - imageSize) / 2;
            if (handle is DrawingHandleScreen screen)
                screen.DrawTextureRect(texture, UIBox2.FromDimensions(topLeft, imageSize));

            return size;
        }
    }
}
