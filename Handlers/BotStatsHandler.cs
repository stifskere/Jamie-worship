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
        if (CommandUsage.ContainsKey(command.CommandName)) CommandUsage[command.CommandName]++;
        else CommandUsage.Add(command.CommandName, 1);
        CommandCount++;
        return Task.CompletedTask;
    }
}