global using static JamieWorshipper.Program;
global using static JamieWorshipper.CustomMethods;
using Discord;
using Discord.WebSocket;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;

namespace JamieWorshipper;

public static class Program
{
    public static readonly CustomClient Client = new(new DiscordSocketConfig{GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.MessageContent}, new CustomClientConfig{EnvPath = "./.env"});
    public static readonly DatabaseHandler DataBase = new("./global.db");
    public static ConfigHandler Config;
    
    public static async Task Main()
    {
        await Client.LoginAsync(TokenType.Bot, Client.Env["TOKEN"]);
        await Client.StartAsync();
        await Task.Delay(-1);
    }
}

public static class CustomMethods
{
    [MustUseReturnValue] public static uint RandomColor() => (uint)new Random().Next(0x0, 0xFFFFFF);
}