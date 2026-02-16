using System.Collections.Frozen;

namespace Content.Server._MC;

public static class MCFormatMessage
{
    public static readonly FrozenDictionary<string, string> Emoji = new Dictionary<string, string>
    {
        { "AmoDuck", "/Textures/_MC/Interface/Emoji/AmoDuck.png" },
        { "beestake", "/Textures/_MC/Interface/Emoji/beestake.png" },
        { "blundir", "/Textures/_MC/Interface/Emoji/blundir.png" },
        { "BLYAT", "/Textures/_MC/Interface/Emoji/BLYAT.png" },
        { "boomcat", "/Textures/_MC/Interface/Emoji/boomcat.png" },
        { "cade", "/Textures/_MC/Interface/Emoji/cade.png" },
        { "catge", "/Textures/_MC/Interface/Emoji/catge.png" },
        { "charlie", "/Textures/_MC/Interface/Emoji/charlie.png" },
        { "china", "/Textures/_MC/Interface/Emoji/china.png" },
        { "clueless", "/Textures/_MC/Interface/Emoji/clueless.png" },
        { "coder", "/Textures/_MC/Interface/Emoji/coder.png" },
        { "codereview", "/Textures/_MC/Interface/Emoji/codereview.png" },
        { "curse220", "/Textures/_MC/Interface/Emoji/curse220.png" },
        { "damn", "/Textures/_MC/Interface/Emoji/damn.png" },
        { "dash", "/Textures/_MC/Interface/Emoji/dash.png" },
        { "defundi", "/Textures/_MC/Interface/Emoji/defundi.png" },
        { "diplocat", "/Textures/_MC/Interface/Emoji/diplocat.png" },
        { "diplosvin", "/Textures/_MC/Interface/Emoji/diplosvin.png" },
        { "disarm", "/Textures/_MC/Interface/Emoji/disarm.png" },
        { "falsesmile", "/Textures/_MC/Interface/Emoji/falsesmile.png" },
        { "fire", "/Textures/_MC/Interface/Emoji/fire.png" },
        { "gagaga", "/Textures/_MC/Interface/Emoji/gagaga.png" },
        { "gay", "/Textures/_MC/Interface/Emoji/gay.png" },
        { "gitupdates", "/Textures/_MC/Interface/Emoji/gitupdates.png" },
        { "goooal", "/Textures/_MC/Interface/Emoji/goooal.png" },
        { "gopcat", "/Textures/_MC/Interface/Emoji/gopcat.png" },
        { "grab", "/Textures/_MC/Interface/Emoji/grab.png" },
        { "gum", "/Textures/_MC/Interface/Emoji/gum.png" },
        { "halal", "/Textures/_MC/Interface/Emoji/halal.png" },
        { "hampter", "/Textures/_MC/Interface/Emoji/hampter.png" },
        { "haram", "/Textures/_MC/Interface/Emoji/haram.png" },
        { "harm", "/Textures/_MC/Interface/Emoji/harm.png" },
        { "help", "/Textures/_MC/Interface/Emoji/help.png" },
        { "HoldingTears", "/Textures/_MC/Interface/Emoji/HoldingTears.png" },
        { "inshallah", "/Textures/_MC/Interface/Emoji/inshallah.png" },
        { "issue", "/Textures/_MC/Interface/Emoji/issue.png" },
        { "kavo", "/Textures/_MC/Interface/Emoji/kavo.png" },
        { "KEKW", "/Textures/_MC/Interface/Emoji/KEKW.png" },
        { "lulz", "/Textures/_MC/Interface/Emoji/lulz.png" },
        { "manwtf", "/Textures/_MC/Interface/Emoji/manwtf.png" },
        { "miner", "/Textures/_MC/Interface/Emoji/miner.png" },
        { "nobalance", "/Textures/_MC/Interface/Emoji/nobalance.png" },
        { "NotLikeThis", "/Textures/_MC/Interface/Emoji/NotLikeThis.png" },
        { "nukie", "/Textures/_MC/Interface/Emoji/nukie.png" },
        { "obida", "/Textures/_MC/Interface/Emoji/obida.png" },
        { "patriot", "/Textures/_MC/Interface/Emoji/patriot.png" },
        { "pig", "/Textures/_MC/Interface/Emoji/pig.png" },
        { "PigDash", "/Textures/_MC/Interface/Emoji/PigDash.png" },
        { "plushie_hampter", "/Textures/_MC/Interface/Emoji/plushie_hampter.png" },
        { "poebat", "/Textures/_MC/Interface/Emoji/poebat.png" },
        { "poggers", "/Textures/_MC/Interface/Emoji/poggers.png" },
        { "rl160", "/Textures/_MC/Interface/Emoji/rl160.png" },
        { "roflanebalo", "/Textures/_MC/Interface/Emoji/roflanebalo.png" },
        { "roflanzdarova", "/Textures/_MC/Interface/Emoji/roflanzdarova.png" },
        { "rosecat", "/Textures/_MC/Interface/Emoji/rosecat.png" },
        { "rouny", "/Textures/_MC/Interface/Emoji/rouny.png" },
        { "sanabi", "/Textures/_MC/Interface/Emoji/sanabi.png" },
        { "smileround", "/Textures/_MC/Interface/Emoji/smileround.png" },
        { "stupidass", "/Textures/_MC/Interface/Emoji/stupidass.png" },
        { "sviin", "/Textures/_MC/Interface/Emoji/sviin.png" },
        { "tatarla", "/Textures/_MC/Interface/Emoji/tatarla.png" },
        { "thundercat", "/Textures/_MC/Interface/Emoji/thundercat.png" },
        { "tile", "/Textures/_MC/Interface/Emoji/tile.png" },
        { "toddler", "/Textures/_MC/Interface/Emoji/toddler.png" },
        { "turret", "/Textures/_MC/Interface/Emoji/turret.png" },
        { "umarines14", "/Textures/_MC/Interface/Emoji/umarines14.png" },
        { "unfortune", "/Textures/_MC/Interface/Emoji/unfortune.png" },
        { "xdd", "/Textures/_MC/Interface/Emoji/xdd.png" },
        { "ZaMamuOtveish", "/Textures/_MC/Interface/Emoji/ZaMamuOtveish.png" },
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
