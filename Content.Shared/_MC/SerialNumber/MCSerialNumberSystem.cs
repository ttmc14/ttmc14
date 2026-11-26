using Content.Shared.Examine;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._MC.SerialNumber;

public sealed class MCSerialNumberSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    private readonly HashSet<string> _issued = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSerialNumberComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MCSerialNumberComponent, ExaminedEvent>(OnExamined);
    }

    private void OnInit(Entity<MCSerialNumberComponent> entity, ref ComponentInit args)
    {
        if (_net.IsClient || (entity.Comp.Number.Length > 0 && _issued.Add(entity.Comp.Number.ToUpperInvariant())))
            return;

        var bytes = new byte[6];
        string number;
        do
        {
            _random.NextBytes(bytes);
            bytes[0] = 0x02;
            number = BitConverter.ToString(bytes).Replace('-', ':');
        } while (!_issued.Add(number));

        entity.Comp.Number = number;
        Dirty(entity);
    }

    private void OnExamined(Entity<MCSerialNumberComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.Number.Length == 0)
            return;

        args.PushMarkup(Loc.GetString("mc-serial-number-examine", ("number", FormattedMessage.EscapeText(entity.Comp.Number))));
    }
}
