using Robust.Shared.Serialization;

namespace Content.Shared._MC.Aura;

[DataDefinition, Serializable, NetSerializable]
public partial struct MCAuraEntry
{
    [DataField]
    public Color Color;

    [DataField]
    public float Width;

    public MCAuraEntry()
    {
        Color = Color.White;
        Width = 2;
    }

    public MCAuraEntry(Color color, float width)
    {
        Color = color;
        Width = width;
    }
}
