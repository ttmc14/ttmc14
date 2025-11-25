using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Shared._MC.Chat;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Marines.Squads;
using Content.Shared._RMC14.Radio;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Speech;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Replays;
using Robust.Shared.Utility;

namespace Content.Server._MC.Chat;

public sealed class MCRadioSystem : MCSharedRadioSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IReplayRecordingManager _replayRecording = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private readonly HashSet<string> _messages = new();
    private readonly SoundSpecifier _radioSound = new SoundPathSpecifier("/Audio/_RMC14/Effects/radiostatic.ogg")
    {
        Params = new AudioParams
        {
            Volume = -8f,
            Variation = 0.1f,
            MaxDistance = 3.75f,
        },
    }; // RMC14

    public override void SendRadioMessage(EntityUid messageSource,
        string message,
        ProtoId<RadioChannelPrototype> channel,
        EntityUid radioSource,
        bool escapeMarkup = true)
    {
        InternalSendRadioMessage(messageSource, message, channel, radioSource, escapeMarkup);
    }

    private void InternalSendRadioMessage(
        EntityUid messageSource,
        string message,
        ProtoId<RadioChannelPrototype> channel,
        EntityUid radioSource,
        bool escapeMarkup = true)
    {
        InternalSendRadioMessage(messageSource, message, _prototype.Index(channel), radioSource, escapeMarkup);
    }

    private void InternalSendRadioMessage(
        EntityUid messageSource,
        string message,
        RadioChannelPrototype channel,
        EntityUid radioSource,
        bool escapeMarkup = true)
    {
        // TODO if radios ever garble / modify messages, feedback-prevention needs to be handled better than this.
        if (!_messages.Add(message))
            return;

        var evt = new TransformSpeakerNameEvent(messageSource, MetaData(messageSource).EntityName);
        RaiseLocalEvent(messageSource, evt);

        var name = evt.VoiceName;
        name = FormattedMessage.EscapeText(name);

        if (TryComp(messageSource, out JobPrefixComponent? prefix))
        {
            var prefixText = (prefix.AdditionalPrefix != null ? $"{Loc.GetString(prefix.AdditionalPrefix.Value)} " : "") + Loc.GetString(prefix.Prefix);
            if (TryComp(messageSource, out SquadMemberComponent? member) &&
                TryComp(member.Squad, out SquadTeamComponent? team) &&
                team.Radio != null &&
                team.Radio != channel.ID)
            {
                name = $"({Name(member.Squad.Value)} {prefixText}) {name}";
            }
            else
            {
                name = $"({prefixText}) {name}";
            }
        }

        SpeechVerbPrototype speech;
        if (evt.SpeechVerb != null && _prototype.TryIndex(evt.SpeechVerb, out var evntProto))
            speech = evntProto;
        else
            speech = _chat.GetSpeechVerb(messageSource, message);

        var content = escapeMarkup
            ? FormattedMessage.EscapeText(message)
            : message;

        // RMC14 increase font size
        var radioFontSize = speech.FontSize;
        if (TryComp<WearingHeadsetComponent>(messageSource, out var wearingHeadset) &&
            TryComp<RMCHeadsetComponent>(wearingHeadset.Headset, out var headsetComp))
        {
            radioFontSize += headsetComp.RadioTextIncrease ?? 0;
        }

        var wrappedMessage = Loc.GetString(speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap",
            ("color", channel.Color),
            ("fontType", speech.FontId),
            ("fontSize", radioFontSize), // RMC14
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", content));

        // most radios are relayed to chat, so lets parse the chat message beforehand
        var chat = new ChatMessage(
            ChatChannel.Radio,
            message,
            wrappedMessage,
            GetNetEntity(messageSource),
            _chatManager.EnsurePlayer(CompOrNull<ActorComponent>(messageSource)?.PlayerSession.UserId)?.Key);
        var chatMsg = new MsgChatMessage { Message = chat };
        var ev = new RadioReceiveEvent(message, messageSource, channel, radioSource, chatMsg);

        var sendAttemptEv = new RadioSendAttemptEvent(channel, radioSource);
        RaiseLocalEvent(ref sendAttemptEv);
        RaiseLocalEvent(radioSource, ref sendAttemptEv);
        var canSend = !sendAttemptEv.Cancelled;

        var sourceMapId = Transform(radioSource).MapID;

        var radioQuery = EntityQueryEnumerator<ActiveRadioComponent, TransformComponent>();
        while (canSend && radioQuery.MoveNext(out var receiver, out var radio, out var transform))
        {
            if (!radio.ReceiveAllChannels)
            {
                if (!radio.Channels.Contains(channel.ID) || (TryComp<IntercomComponent>(receiver, out var intercom) &&
                                                             !intercom.SupportedChannels.Contains(channel.ID)))
                    continue;
            }

            if (!channel.LongRange && transform.MapID != sourceMapId && !radio.GlobalReceive)
                continue;

            // check if message can be sent to specific receiver
            var attemptEv = new RadioReceiveAttemptEvent(channel, radioSource, receiver);
            RaiseLocalEvent(ref attemptEv);
            RaiseLocalEvent(receiver, ref attemptEv);
            if (attemptEv.Cancelled)
                continue;

            // send the message
            RaiseLocalEvent(receiver, ref ev);
        }

        if (canSend &&
            !HasComp<XenoComponent>(messageSource) &&
            HasComp<RMCHeadsetComponent>(radioSource))
        {
            var filter = Filter.Pvs(messageSource).RemoveWhereAttachedEntity(HasComp<XenoComponent>);
            _audio.PlayEntity(_radioSound, filter, messageSource, false); // RMC14
        }

        if (name != Name(messageSource))
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} as {name} on {channel.LocalizedName}: {message}");
        else
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} on {channel.LocalizedName}: {message}");

        _replayRecording.RecordServerMessage(chat);
        _messages.Remove(message);
    }
}
