using Discord;
using Discord.Interactions;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;

namespace JamieWorshipper.Commands;

[Group("blacklist", "Black list related commands")]
public class Blacklist : InteractionModuleBase<SocketInteractionContext>
{
    private async Task<bool> CanInteract(IUser target)
    {
        if (!target.IsModerator()) return true;
        await RespondAsync("❌ You cannot blacklist/un-blacklist yourself nor any moderator. ❌", ephemeral: true);
        return false;
    }
    
    [UserCommand("Blacklist"), UsedImplicitly, OnlyModerators(ModeratorsSelection.AllMods, "❌ You cannot blacklist worshippers, you require to be a bot moderator to do so. ❌")]
    public async Task BlackListAsync(IUser target)
    {
        if (!await CanInteract(target)) return;

        if (!target.IsOnBlackList())
        {
            DataBase.RunSqliteCommandAllRows($"INSERT OR IGNORE INTO BlackListedUsers(UserId, AuthorId) VALUES({target.Id}, {Context.User.Id})");
            await RespondAsync($"{target.Mention} was blacklisted successfully, they won't be able to send worships until un-blacklist.", ephemeral: true);
        }
        else await RespondAsync($"{target.Mention} is already blacklisted, they can't send worships until un-blacklist", ephemeral: true);
    }

    [UserCommand("Whitelist"), UsedImplicitly, OnlyModerators(ModeratorsSelection.AllMods, "❌ You cannot whitelist worshippers, you require to be a bot moderator to do so. ❌")]
    public async Task WhiteListAsync(IUser target)
    {
        if (!await CanInteract(target)) return;

        if (target.IsOnBlackList())
        {
            DataBase.RunSqliteCommandAllRows($"DELETE FROM BlackListedUsers WHERE UserId = {target.Id}");
            await RespondAsync($"{target.Mention} was removed from the blacklist successfully, they now will be able to worship Jamie.", ephemeral: true);
        }
        else await RespondAsync($"{target.Mention} is already not blacklisted", ephemeral: true);
    }
    
    [SlashCommand("view", "View blacklisted users"), UsedImplicitly,
     CommandCooldown(15), OnlyModerators(ModeratorsSelection.AllMods, "❌ Only moderators can view blacklisted users due to privacy reasons ❌")]
    public Task ViewBlackListAsync()
    {
        List<List<object>> dbBlackList = DataBase.RunSqliteCommandAllRows("SELECT UserId, AuthorId FROM BlackListedUsers");
        _ = new PagedEmbedHandler<List<object>>(new()
        {
            Prefix = "BlackList", 
            CommandName = "/blacklist view",
            ErrorEmptyTitle = "The blacklist is empty",
            ErrorEmptyDescription = "There isn't any blacklisted user, `right click > apps > blacklist` a user to blacklist them.",
            ListEmbedTitle = "Here are the blacklisted users"
        }, 
            dbBlackList, Context, async iteration =>
            {
                IUser currentUser = await Client.GetUserAsync((ulong)(long)iteration[0]);
                IUser? currentModerator = null;
                if (iteration[1] is not DBNull) currentModerator = await Client.GetUserAsync((ulong)(long)iteration[1]);
                
                return new EmbedFieldBuilder()
                    .WithName($"User: {currentUser}")
                    .WithValue($"**ID:** `{currentUser.Id}`\n**Joined discord:** <t:{currentUser.CreatedAt.ToUnixTimeSeconds()}:F>\n**Moderator:** {(currentModerator != null ? $"{currentModerator}\n**Moderator ID:** `{currentModerator.Id}`" : "Moderator is not known")}");
            }
        );
        return Task.CompletedTask;
    }
}