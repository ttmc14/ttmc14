using Content.Server.Chat.Managers;
using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Robust.Shared.Player;

namespace Content.Server._MC.Rules;

public sealed class MCRuleStartValidationSystem : EntitySystem
{
    [Dependency] private readonly IServerPreferencesManager _preferences = null!;
    [Dependency] private readonly IChatManager _chatManager = null!;

    public bool TryValidateXenoRequirements(
        IReadOnlyList<ICommonSession> players,
        string xenoJob,
        string shrikeJob,
        int minPlayers,
        int minXenoCandidates,
        out int foundXenoCandidates,
        out string? failReason)
    {
        foundXenoCandidates = 0;
        failReason = null;

        if (players.Count < minPlayers)
        {
            failReason = $"Невозможно запустить режим. Требуется как минимум {minPlayers} игрока, но у нас есть {players.Count}.";
            return false;
        }

        foreach (var player in players)
        {
            if (!_preferences.TryGetCachedPreferences(player.UserId, out var preferences))
                continue;

            if (preferences.GetProfile(preferences.SelectedCharacterIndex) is not HumanoidCharacterProfile profile)
                continue;

            if (IsXenoCandidate(profile, xenoJob, shrikeJob))
                foundXenoCandidates++;
        }

        if (foundXenoCandidates >= minXenoCandidates)
            return true;

        failReason = $"Невозможно запустить режим. Требуется как минимум {minXenoCandidates} ксено-игрок, но у нас есть {foundXenoCandidates}.";
        return false;
    }

    public void AnnounceFail(string? msg)
    {
        if (string.IsNullOrEmpty(msg))
            return;

        _chatManager.SendAdminAnnouncement(msg);
        _chatManager.DispatchServerAnnouncement(msg);
    }

    private static bool IsXenoCandidate(
        HumanoidCharacterProfile profile,
        string xenoJob,
        string shrikeJob)
    {
        return
            profile.JobPriorities.TryGetValue(xenoJob, out var xenoPriority) && xenoPriority > JobPriority.Never ||
            profile.JobPriorities.TryGetValue(shrikeJob, out var shrikePriority) && shrikePriority > JobPriority.Never;
    }
}
