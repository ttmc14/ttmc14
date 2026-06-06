using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.Core;

public sealed partial class MCArmorSystem
{
    private static readonly ProtoId<TagPrototype> TagMelee = "MCDamageMelee";
    private static readonly ProtoId<TagPrototype> TagBullet = "MCDamageBullet";
    private static readonly ProtoId<TagPrototype> TagLaser = "MCDamageLaser";
    private static readonly ProtoId<TagPrototype> TagEnergy = "MCDamageEnergy";
    private static readonly ProtoId<TagPrototype> TagBomb = "MCDamageBomb";
    private static readonly ProtoId<TagPrototype> TagBio = "MCDamageBio";
    private static readonly ProtoId<TagPrototype> TagFire = "MCDamageFire";
    private static readonly ProtoId<TagPrototype> TagAcid = "MCDamageAcid";
}
