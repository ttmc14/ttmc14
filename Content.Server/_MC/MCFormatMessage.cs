using System.Collections.Frozen;

namespace Content.Server._MC;

public sealed class MCFormatMessage
{
    private static readonly FrozenDictionary<string, string> Emoji = new Dictionary<string, string>
    {
        { "bib", "/Textures/_MC/Mobs/Xenos/rafik.rsi/resting.png" },
    }.ToFrozenDictionary();

    public static string ApplyEmoji(string text)
    {
        foreach (var (name, path) in Emoji)
        {
            text = text.Replace($":{name}:", $"[mcsprite=\"{path}\"]");
        }

        return text;
    }
}
