namespace Content.Shared._MC.Marine.Customization.Events;

[ByRefEvent]
public record struct MCCustomizationApplyEvent(string Key, MCCustomizationVariationData Data);
