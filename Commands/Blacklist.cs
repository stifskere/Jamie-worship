using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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

    private static readonly Dictionary<ulong, IUser> Targets = new();

    [UserCommand("Blacklist"), UsedImplicitly, OnlyModerators(ModeratorsSelection.AllMods, "❌ You cannot blacklist worshippers, you require to be a bot moderator to do so. ❌")]
    public async Task BlackListAsync(IUser target)
    {
        if (!await CanInteract(target)) return;

        if (!target.IsOnBlackList())
        {
            Targets.Add(Context.User.Id, target);

            ModalBuilder modal = new ModalBuilder()
                .WithTitle("Blacklist")
                .WithCustomId("BlacklistModal")
                .AddTextInput("Reason", "reason", placeholder: "optional");
            
            await RespondWithModalAsync(modal.Build());
            return;
        }
        
        await RespondAsync($"{target.Mention} is already blacklisted, they can't send worships until un-blacklist", ephemeral: true);
    }

    [UserCommand("Whitelist"), UsedImplicitly, OnlyModerators(ModeratorsSelection.AllMods, "❌ You cannot whitelist worshippers, you require to be a bot moderator to do so. ❌")]
    public async Task WhiteListAsync(IUser target)
    {
        if (!await CanInteract(target)) return;

        if (target.IsOnBlackList())
        {
            DataBase.RunSqliteCommandAllRows($"DELETE FROM BlackListedUsers WHERE UserId = {target.Id}");
            await RespondAsync($"{target.Mention} was whitelisted successfully, they can worship {Config.Jamie.Username} now.", ephemeral: true);
            return;
        }
        
        await RespondAsync($"{target.Mention} is already not blacklisted", ephemeral: true);
    }

    [Event(EventTypes.ModalSubmitted), UsedImplicitly]
    public static async Task SubmitModalAsync(SocketModal modal)
    {
        if (modal.Data.CustomId != "BlacklistModal") return;
        
        string reason = modal.Data.Components.First(x => x.CustomId == "reason").Value;
        IUser target = Targets[modal.User.Id];

        DataBase.RunSqliteCommandAllRows($"INSERT OR IGNORE INTO BlackListedUsers(UserId, AuthorId, Reason) VALUES({target.Id}, {modal.User.Id}, @0)", reason);
        
        await modal.RespondAsync($"{target.Mention} was blacklisted successfully, they can't worship {Config.Jamie.Username} anymore until they are whitelisted again.", ephemeral: true);
        Targets.Remove(modal.User.Id);
    }
    
    [SlashCommand("view", "View blacklisted users"), UsedImplicitly,
     CommandCooldown(15), OnlyModerators(ModeratorsSelection.AllMods, "❌ Only moderators can view blacklisted users due to privacy reasons ❌")]
    public Task ViewBlackListAsync()
    {
        List<List<object>> dbBlackList = DataBase.RunSqliteCommandAllRows("SELECT UserId, AuthorId, Reason FROM BlackListedUsers");
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
                IUser? currentModerator = iteration[1] is not DBNull ? await Client.GetUserAsync((ulong)(long)iteration[1]) : null;

                return new EmbedFieldBuilder()
                    .WithName($"User: {currentUser}")
                    .WithValue($"**ID:** `{currentUser.Id}`\n**Moderator:** {(currentModerator != null ? $"{currentModerator}\n**Moderator ID:** `{currentModerator.Id}`" : "Moderator is not known")}\n**Reason:** {(iteration[2] is not DBNull ? iteration[2] : "no reason defined.")}");
            }
        );
        return Task.CompletedTask;
    }
}