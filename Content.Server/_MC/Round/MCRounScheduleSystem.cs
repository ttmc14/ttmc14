using System.Linq;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Shared._MC;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Server._MC.Round;

public sealed class MCRoundScheduleSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;

    #region Config

    private bool _enabled;
    private TimeZoneInfo _timezoneInfo = TimeZoneInfo.Local;
    private TimeSpan _updateFrequency;
    private DayOfWeek[] _days = [];
    private TimeSpan _start;
    private TimeSpan _end;

    #endregion

    private TimeSpan _nextUpdateTime = TimeSpan.Zero;

    private DateTime DateNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timezoneInfo);

    public override void Initialize()
    {
        base.Initialize();

#if FULL_RELEASE
        _configuration.OnValueChanged(MCConfigVars.MCRoundSchedule, value => _enabled = value, true);
#else
        _enabled = false;
#endif

        _configuration.OnValueChanged(MCConfigVars.MCRoundScheduleTimezone, value => _timezoneInfo = ParseTimeZoneInfo(value), true);
        _configuration.OnValueChanged(MCConfigVars.MCRoundScheduleUpdateFrequency, value => _updateFrequency = TimeSpan.FromSeconds(value), true);
        _configuration.OnValueChanged(MCConfigVars.MCRoundScheduleDays, v => _days = ParseDays(v), true);
        _configuration.OnValueChanged(MCConfigVars.MCRoundScheduleStart, v => _start = ParseTime(v), true);
        _configuration.OnValueChanged(MCConfigVars.MCRoundScheduleEnd, v => _end = ParseTime(v), true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_enabled || _timing.CurTime < _nextUpdateTime)
            return;

        _nextUpdateTime = _timing.CurTime + _updateFrequency;

        if (_ticker.Paused && CanStartRound())
            _ticker.TogglePause();

        if (_ticker.Paused || CanStartRound())
            return;

        _ticker.TogglePause();
        _roundEnd.EndRound();
    }

    public bool CanStartRound()
    {
        var day = DateNow.DayOfWeek;
        var time = DateNow.TimeOfDay;

        var allowedDay = _days.Contains(day);
        var allowedTime = IsInTimeRange(time, _start, _end);

        return allowedDay && allowedTime;
    }

    private DayOfWeek[] ParseDays(string input)
    {
        return input
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(key =>
            {
                if (Enum.TryParse<DayOfWeek>(key, true, out var result))
                    return result;

                Log.Warning($"Unable to recognize the day of the week \"{key}\"");
                return (DayOfWeek?) null;
            })
            .Where(day => day is not null)
            .Cast<DayOfWeek>()
            .ToArray();
    }

    private static TimeZoneInfo ParseTimeZoneInfo(string input)
    {
        return input == string.Empty
            ? TimeZoneInfo.Local
            : TimeZoneInfo.FindSystemTimeZoneById(input);
    }

    private static TimeSpan ParseTime(string input)
    {
        return TimeSpan.TryParse(input, out var result) ? result : TimeSpan.Zero;
    }

    private static bool IsInTimeRange(TimeSpan time, TimeSpan start, TimeSpan end)
    {
        return end <= start ? time >= start || time < end : time >= start && time < end;
    }

}
