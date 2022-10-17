using Discord.WebSocket;
using JetBrains.Annotations;

namespace JamieWorshipper.Handlers;

public static class BotStatsHandler
{
    public static Dictionary<string, int> CommandUsage { get; } = new();
    public static int WorshipsNum { get; internal set; }
    public static int CommandCount { get; internal set; }
    public static DateTimeOffset Uptime { get; } = DateTimeOffset.Now;
    
    [Event(EventTypes.SlashCommandExecuted), UsedImplicitly]
    public static Task SlashCommandExecutedEvent(SocketSlashCommand command)
    {
        UpdateUsage($"{command.CommandName} {string.Join(" ", command.Data.Options.Select(option => option.Name))}");
        return Task.CompletedTask;
    }

    [Event(EventTypes.UserCommandExecuted), UsedImplicitly]
    public static Task UserCommandExecutedEvent(SocketUserCommand command)
    {
        UpdateUsage($"{command.CommandName}");
        return Task.CompletedTask;
    }

    private static void UpdateUsage(string dictName)
    {
        if (CommandUsage.ContainsKey(dictName)) CommandUsage[dictName]++;
        else CommandUsage.Add(dictName, 1);
        CommandCount++;
    }
}