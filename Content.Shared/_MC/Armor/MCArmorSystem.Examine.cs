using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Armor;

public sealed partial class MCArmorSystem
{
    private const bool ShowZeros = true;

    private void OnArmorVerbExamine(Entity<MCArmorComponent> entity, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !entity.Comp.ShowExamine)
            return;

        var examineMarkup = GetArmorExamine(entity);
        _examine.AddDetailedExamineVerb(
            args,
            entity,
            examineMarkup,
            Loc.GetString("mc-armor-examinable-verb-text"),
            "/Textures/Interface/Actions/actions_fakemindshield.rsi/icon-on.png",
            Loc.GetString("mc-armor-examinable-verb-message")
        );
    }

    private FormattedMessage GetArmorExamine(EntityUid entityUid)
    {
        if (GetSoftArmor(entityUid) is not { } armor)
            return new FormattedMessage();

        var message = new FormattedMessage();
        message.AddMarkupOrThrow(Loc.GetString("mc-armor-examine-title"));

        var armorRatings = new[]
        {
            (Loc.GetString("mc-armor-melee"), armor.Melee),
            (Loc.GetString("mc-armor-bullet"), armor.Bullet),
            (Loc.GetString("mc-armor-laser"), armor.Laser),
            (Loc.GetString("mc-armor-energy"), armor.Energy),
            (Loc.GetString("mc-armor-bomb"), armor.Bomb),
            (Loc.GetString("mc-armor-bio"), armor.Bio),
            (Loc.GetString("mc-armor-fire"), armor.Fire),
            (Loc.GetString("mc-armor-acid"), armor.Acid),
            (Loc.GetString("mc-armor-fall"), armor.Fall),
        };

        foreach (var (text, value) in armorRatings)
        {
            if (value == 0 && !ShowZeros)
                continue;

            message.PushNewline();
            message.AddMarkupOrThrow(Loc.GetString(
                "mc-armor-examine-armor",
                ("text", text),
                ("value", value)
            ));
        }

        return message;
    }
}
