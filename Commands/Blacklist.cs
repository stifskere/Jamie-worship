using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace JamieWorshipper.Commands;

[UsedImplicitly]
public class Blacklist : InteractionModuleBase<SocketInteractionContext>
{
    private static List<object> GetBlackListData(IUser target) => DataBase.RunSqliteCommandFirstRow($"SELECT Id FROM BlackListedUsers WHERE Id = {target.Id}");

    private async Task<bool> CanInteract(IUser target, IUser user)
    {
        if (!Config.Moderators.Contains(user.Id))
        {
            await RespondAsync("You cannot blacklist/un-blacklist worshippers, you need to be a JamieWorship moderator.", ephemeral: true);
            return false;
        }

        if (Config.Moderators.Contains(target.Id))
        {
            await RespondAsync("You cannot blacklist/un-blacklist yourself or any moderator.", ephemeral: true);
            return false;
        }

        return true;
    }
    
    [UserCommand("Blacklist"), UsedImplicitly]
    public async Task BlackListAsync(IUser target)
    {
        if (!await CanInteract(target, Context.User)) return;

        List<object> data = GetBlackListData(target);
        
        if (data.Count == 0)
        {
            DataBase.RunSqliteCommandAllRows($"INSERT OR IGNORE INTO BlackListedUsers(Id) VALUES({target.Id})");
            await RespondAsync($"{target.Mention} was blacklisted successfully, they won't be able to send worships until un-blacklist.", ephemeral: true);
        }
        else await RespondAsync($"{target.Mention} is already blacklisted, they can't send worships until un-blacklist", ephemeral: true);
    }

    [UserCommand("Whitelist"), UsedImplicitly]
    public async Task WhiteListAsync(IUser target)
    {
        if (!await CanInteract(target, Context.User)) return;

        List<object> data = GetBlackListData(target);
        
        if (data.Count != 0)
        {
            DataBase.RunSqliteCommandAllRows($"DELETE FROM BlackListedUsers WHERE Id = {target.Id}");
            await RespondAsync($"{target.Mention} was removed from the blacklist successfully, they now will be able to worship Jamie.", ephemeral: true);
        }
        else await RespondAsync($"{target.Mention} is already not blacklisted", ephemeral: true);
    }
}