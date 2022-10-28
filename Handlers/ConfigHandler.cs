using System.Reflection;
using Discord;
using JetBrains.Annotations;

namespace JamieWorshipper.Handlers;

public class ConfigHandler
{
    public ITextChannel MessagesChannel { get; private set; } = null!;
    public IGuild MainGuild { get; private set; } = null!;
    public IUser Jamie { get; private set; } = null!;
    public ulong[] Moderators { get; private set; } = Array.Empty<ulong>();

    public IRole CloseFriendsRole { get; set; } = null!;

    [UsedImplicitly] private ulong _privateMessagesChannel = 1;
    [UsedImplicitly] private ulong _privateMainGuild = 1;
    [UsedImplicitly] private ulong _privateJamieId = 1;
    [UsedImplicitly] private ulong _privateCloseFriendsRole = 1;

    public bool Ready;
    
    public ConfigHandler() => new Thread(() => ReloadConfig().Wait()).Start();

    public async Task<Exception?> ReloadConfig()
    {
        Exception? returnException = null;
        try
        {
            while (Client.ConnectionState != ConnectionState.Connected) {}
            foreach (List<object> data in DataBase.RunSqliteCommandAllRows("SELECT ConfigKey, ConfigValue FROM BotConfig"))
                GetType()
                    .GetField($"_private{(string)data[0]}", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)!
                    .SetValue(this, ulong.Parse((string)data[1]));

            MainGuild = Client.GetGuild(_privateMainGuild);
            MessagesChannel = (ITextChannel)await Client.GetChannelAsync(_privateMessagesChannel);
            Jamie = await Client.GetUserAsync(_privateJamieId);
            Moderators = Client.Env["MODERATORS"].Split(",").Select(ulong.Parse).ToArray();
            CloseFriendsRole = MainGuild.Roles.First(r => r.Id == _privateCloseFriendsRole);
        }
        catch (Exception ex)
        {
            returnException = ex;
        }
        finally
        {
            Ready = true;
        }
        
        return returnException;
    }
}