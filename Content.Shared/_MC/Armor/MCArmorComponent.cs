using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Armor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool ShowExamine = true;

    [DataField, AutoNetworkedField]
    public MCArmorDefinition Soft;
}

[DataDefinition, Serializable, NetSerializable]
public partial struct MCArmorDefinition : IEquatable<MCArmorDefinition>
{
    [DataField]
    public int Melee;

    [DataField]
    public int Bullet;

    [DataField]
    public int Laser;

    [DataField]
    public int Energy;

    [DataField]
    public int Bomb;

    [DataField]
    public int Bio;

    [DataField]
    public int Fire;

    [DataField]
    public int Acid;

    public bool Equals(MCArmorDefinition other)
    {
        return Melee == other.Melee && Bullet == other.Bullet
                                    && Laser == other.Laser
                                    && Energy == other.Energy
                                    && Bomb == other.Bomb
                                    && Bio == other.Bio
                                    && Fire == other.Fire
                                    && Acid == other.Acid;
    }

    public override bool Equals(object? obj)
    {
        return obj is MCArmorDefinition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Melee, Bullet, Laser, Energy, Bomb, Bio, Fire, Acid);
    }

    public static MCArmorDefinition operator +(MCArmorDefinition left, int right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee + right,
            Bullet = left.Bullet + right,
            Laser = left.Laser + right,
            Energy = left.Energy + right,
            Bomb = left.Bomb + right,
            Bio = left.Bio + right,
            Fire = left.Fire + right,
            Acid = left.Acid + right,
        };
    }

    public static MCArmorDefinition operator +(MCArmorDefinition left, MCArmorDefinition right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee + right.Melee,
            Bullet = left.Bullet + right.Bullet,
            Laser = left.Laser + right.Laser,
            Energy = left.Energy + right.Energy,
            Bomb = left.Bomb + right.Bomb,
            Bio = left.Bio + right.Bio,
            Fire = left.Fire + right.Fire,
            Acid = left.Acid + right.Acid,
        };
    }

    public static MCArmorDefinition operator -(MCArmorDefinition left, int right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee - right,
            Bullet = left.Bullet - right,
            Laser = left.Laser - right,
            Energy = left.Energy - right,
            Bomb = left.Bomb - right,
            Bio = left.Bio - right,
            Fire = left.Fire - right,
            Acid = left.Acid - right,
        };
    }

    public static MCArmorDefinition operator -(MCArmorDefinition left, MCArmorDefinition right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee - right.Melee,
            Bullet = left.Bullet - right.Bullet,
            Laser = left.Laser - right.Laser,
            Energy = left.Energy - right.Energy,
            Bomb = left.Bomb - right.Bomb,
            Bio = left.Bio - right.Bio,
            Fire = left.Fire - right.Fire,
            Acid = left.Acid - right.Acid,
        };
    }
}
