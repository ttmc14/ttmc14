using System.Text;
using Content.Shared.Chemistry.Reagent;

namespace Content.Shared._MC.Utilities;

public static class StringBuilderExtension
{
    public static StringBuilder AppendReagent(this StringBuilder stringBuilder, ReagentPrototype prototype)
    {
        stringBuilder.Append("[color=");
        stringBuilder.Append(prototype.SubstanceColor.ToHexNoAlpha());
        stringBuilder.Append(']');
        stringBuilder.Append(prototype.LocalizedName);
        stringBuilder.Append("[/color]");
        return stringBuilder;
    }

    public static StringBuilder AppendLineReagent(this StringBuilder stringBuilder, ReagentPrototype prototype)
    {
        stringBuilder.AppendReagent(prototype);
        stringBuilder.AppendLine();
        return stringBuilder;
    }
}
