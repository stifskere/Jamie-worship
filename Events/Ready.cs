using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Discord;
using Discord.Interactions;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;

namespace JamieWorshipper.Events;

public static class Ready
{
    [Event(EventTypes.Ready), UsedImplicitly]
    public static async Task ReadyEvent()
    {
        if (!Client.RunOnce) return;
        
        InteractionService commands = new(Client);

        Client.InteractionCreated += async interaction =>  await commands.ExecuteCommandAsync(new SocketInteractionContext(Client, interaction), null);
        commands.SlashCommandExecuted += (_, _, result) => {if(result.Error != null && result is not PreconditionResult) Console.WriteLine(result); return Task.CompletedTask;};
        await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
        await commands.RegisterCommandsGloballyAsync();
        Config = new ConfigHandler();
        new Thread(CreateDatabasesThread).Start();
        new Thread(StatusThread).Start();
        BotStatsHandler.Uptime = DateTimeOffset.Now;
        //For pterodactyl, y'all can remove, i won't.
        Console.WriteLine("Started");
    }

    [DoesNotReturn]
    private static async void StatusThread()
    {
        while (!Config.Ready) {}
        
        KeyValuePair<string, ActivityType>[] possibleStatuses =
        {
            new(Config.Jamie.Username, ActivityType.Watching),
            new($"how worshippers worship {Config.Jamie.Username}", ActivityType.Watching),
            new($"with {Config.Jamie.Username}", ActivityType.Playing),
            new($"{Config.Jamie.Username} scream about his DMS", ActivityType.Listening)
        };
        
        while (true)
            foreach (KeyValuePair<string, ActivityType> current in possibleStatuses)
            {
                await Client.SetActivityAsync(new Game(current.Key, current.Value));
                Thread.Sleep(TimeSpan.FromHours(3));
            }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void CreateDatabasesThread()
    {
        DataBase.RunSqliteCommandAllRows(@"
            CREATE TABLE IF NOT EXISTS Worshippers(Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId INTEGER NOT NULL, Worship VARCHAR NOT NULL, Guild VARCHAR NOT NULL, unique(Id));
            CREATE TABLE IF NOT EXISTS JamieInfo(InfoKey VARCHAR NOT NULL PRIMARY KEY, InfoValue VARCHAR NOT NULL, unique(InfoKey));
            CREATE TABLE IF NOT EXISTS BlackListedUsers(UserId INTEGER PRIMARY KEY NOT NULL, AuthorId INTEGER, unique(UserId));
            CREATE TABLE IF NOT EXISTS BotConfig(ConfigKey VARCHAR NOT NULL PRIMARY KEY, ConfigValue VARCHAR NOT NULL, unique(ConfigKey));
            ");

        DataBase.RunSqliteCommandAllRows(@"
            INSERT OR IGNORE INTO JamieInfo(InfoKey, InfoValue) VALUES('MessageNum', '0'), ('LastMessage', 'Any ""last message"" sent yet');
            INSERT OR IGNORE INTO BotConfig(ConfigKey, ConfigValue) VALUES('MessagesChannel', '976832701015420998'), ('MainGuild', '976149800447770624'), ('JamieId', '394127601398054912'), ('CloseFriendsRole', '976175903371571220');
            ");
    }
        
}