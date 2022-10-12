global using static JamieWorshiper.Program;
global using static JamieWorshiper.CustomMethods;

using Discord;
using Discord.WebSocket;
using JamieWorshiper.Handlers;
using JetBrains.Annotations;

namespace JamieWorshiper;

public static class Program
{
    public static readonly CustomClient Client = new(new DiscordSocketConfig{GatewayIntents = GatewayIntents.All}, new CustomClientConfig{EnvPath = "./.env"});
    public static readonly DatabaseHandler DataBase = new("./global.db");
    public static readonly ConfigHandler Config = new();
    
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