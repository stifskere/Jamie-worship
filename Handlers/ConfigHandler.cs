using System.Reflection;
using Discord;
using JetBrains.Annotations;

namespace JamieWorshipper.Handlers;

public class ConfigHandler
{
    public ITextChannel MessagesChannel { get; internal set; } = null!;
    public IGuild MainGuild { get; internal set; } = null!;
    public IUser Jamie { get; internal set; } = null!;
    public ulong[] Moderators { get; internal set; } = Array.Empty<ulong>();

    [UsedImplicitly] private ulong _privateMessagesChannel;
    [UsedImplicitly] private ulong _privateMainGuild;
    [UsedImplicitly] private ulong _privateJamieId;
    
    public ConfigHandler() => new Thread(ReloadConfig).Start();

    public async void ReloadConfig()
    {
        while (Client.ConnectionState != ConnectionState.Connected) { }
        foreach (List<object> data in DataBase.RunSqliteCommandAllRows("SELECT ConfigKey, ConfigValue FROM BotConfig"))
            GetType().GetField($"_private{(string)data[0]}", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)!.SetValue(this, ulong.Parse((string)data[1]));

        MainGuild = Client.GetGuild(_privateMainGuild);
        MessagesChannel = (ITextChannel)await Client.GetChannelAsync(_privateMessagesChannel);
        Jamie = await Client.GetUserAsync(_privateJamieId);
        
        Moderators = Client.Env["MODERATORS"].Split(",").Select(ulong.Parse).ToArray();
    }
}