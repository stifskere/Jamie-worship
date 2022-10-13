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
        string dictName = $"{command.CommandName} {command.Data.Options.First().Name}";
        if (CommandUsage.ContainsKey(dictName)) CommandUsage[dictName]++;
        else CommandUsage.Add(dictName, 1);
        CommandCount++;
        return Task.CompletedTask;
    }
}