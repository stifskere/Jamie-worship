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
        commands.SlashCommandExecuted += (_, _, result) => {if(result.Error != null) Console.WriteLine(result); return Task.CompletedTask;};
        await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
        await commands.RegisterCommandsGloballyAsync();
        new Thread(CreateDatabasesThread).Start();
        await Client.SetActivityAsync(new Game("The jamie worship game but in beta"));
        //For pterodactyl, y'all can remove, i won't.
        Console.WriteLine("Started");
        
    }

    private static void CreateDatabasesThread()
    {
        DataBase.RunSqliteCommandAllRows(@"
            CREATE TABLE IF NOT EXISTS Worshippers(Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId INTEGER NOT NULL, Worship VARCHAR NOT NULL, Guild VARCHAR NOT NULL, unique(Id));
            CREATE TABLE IF NOT EXISTS JamieInfo(InfoKey VARCHAR NOT NULL PRIMARY KEY, InfoValue VARCHAR NOT NULL, unique(InfoKey));
            CREATE TABLE IF NOT EXISTS BlackListedUsers(Id INTEGER PRIMARY KEY NOT NULL, unique(Id));
            CREATE TABLE IF NOT EXISTS BotConfig(ConfigKey VARCHAR NOT NULL PRIMARY KEY, ConfigValue VARCHAR NOT NULL, unique(ConfigKey));
            ");

        DataBase.RunSqliteCommandAllRows(@"
            INSERT OR IGNORE INTO JamieInfo(InfoKey, InfoValue) VALUES('MessageNum', '0'), ('LastMessage', 'Any ""last message"" sent yet');
            INSERT OR IGNORE INTO BotConfig(ConfigKey, ConfigValue) VALUES('MessagesChannel', '976832701015420998'), ('MainGuild', '976149800447770624'), ('JamieId', '394127601398054912');
            ");
    }
        
}