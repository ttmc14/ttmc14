using Content.Shared._RMC14.CCVar;
using Content.Shared._RMC14.NightVision;
using Content.Shared._RMC14.Xenonids;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server._RMC14.NightVision;

public sealed class NightVisionSystem : SharedNightVisionSystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly INetConfigurationManager _netConfig = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        if (!HasComp<XenoComponent>(args.Entity))
            return;

        if (!TryComp(args.Entity, out NightVisionComponent? nightVision))
            return;

        var stateValue = _netConfig.GetClientCVar(args.Player.Channel, RMCCVars.RMCXenoDefaultNightVision);
        var state = ToNightVisionState(stateValue);
        SetState((args.Entity, nightVision), state);
    }

    protected override void NightVisionChanged(Entity<NightVisionComponent> ent)
    {
        if (!ent.Comp.DrawFov && TryComp<EyeComponent>(ent, out var eye))
            _eye.SetDrawFov(ent, ent.Comp.State == NightVisionState.Off, eye);
    }

    protected override void NightVisionRemoved(Entity<NightVisionComponent> ent)
    {
        if (!ent.Comp.DrawFov && !TerminatingOrDeleted(ent) && TryComp<EyeComponent>(ent, out var eye))
            _eye.SetDrawFov(ent, true, eye);
    }

    private static NightVisionState ToNightVisionState(int value)
    {
        return value switch
        {
            (int) NightVisionState.Off => NightVisionState.Off,
            (int) NightVisionState.Half => NightVisionState.Half,
            (int) NightVisionState.Full => NightVisionState.Full,
            _ => NightVisionState.Half,
        };
    }
}
