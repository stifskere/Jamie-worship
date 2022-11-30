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
    
    [UserCommand("Blacklist"), UsedImplicitly, OnlyModerators(ModeratorsSelection.AllMods, "❌ You cannot blacklist worshippers, you require to be a bot moderator to do so. ❌")]
    public async Task BlackListAsync(IUser target)
    {
        if (!await CanInteract(target)) return;

        if (!target.IsOnBlackList())
        {
            DataBase.RunSqliteCommandAllRows($"INSERT OR IGNORE INTO BlackListedUsers(Id) VALUES({target.Id})");
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
            DataBase.RunSqliteCommandAllRows($"DELETE FROM BlackListedUsers WHERE Id = {target.Id}");
            await RespondAsync($"{target.Mention} was removed from the blacklist successfully, they now will be able to worship Jamie.", ephemeral: true);
        }
        else await RespondAsync($"{target.Mention} is already not blacklisted", ephemeral: true);
    }

    private static List<List<object>> _blackList = DataBase.RunSqliteCommandAllRows("SELECT Id FROM BlackListedUsers");
    private static readonly Dictionary<ulong, ViewBlackListInstance> ViewBlackListInstances = new();

    private static EmbedBuilder GetPageEmbed(int start, uint embedColor) => new EmbedBuilder()
        .WithTitle("Here are all the blacklisted users")
        .WithFields(_blackList.Skip(start).Take(5).Select(async s =>
        {
            IUser user = await Client.GetUserAsync((ulong)(long)s[0]);
            return new EmbedFieldBuilder()
                .WithName($"User: {user.ToString() ?? "Not found#0000"}")
                .WithValue($"**ID:** `{user.Id}`\n**Joined discord:** <t:{user.CreatedAt.ToUnixTimeSeconds()}:F>");
        }).Select(r => r.Result))
        .WithColor(embedColor)
        .WithFooter($"Showing {start + 1}-{start + 6} out of {_blackList.Count}");

    private struct ViewBlackListInstance
    {
        public int CurrentIndex { get; set; }
        public IUser User { get; init; }
        public uint EmbedColor { get; init; }
    }
    
    private static readonly ButtonBuilder BackButton = new ButtonBuilder()
        .WithStyle(ButtonStyle.Secondary)
        .WithLabel("Back")
        .WithEmote(new Emoji("⬅"))
        .WithCustomId("BlackListBack");
        
    private static readonly ButtonBuilder ForwardButton = new ButtonBuilder()
        .WithStyle(ButtonStyle.Secondary)
        .WithLabel("Forward")
        .WithEmote(new Emoji("➡"))
        .WithCustomId("BlackListForward");

    [Event(EventTypes.ButtonExecuted), UsedImplicitly]
    public static async Task ViewBlackListButtonsEvent(SocketMessageComponent component)
    {
        if (component.Data.CustomId is not ("BlackListBack" or "BlackListForward")) return;

        await component.DeferAsync(ephemeral: true);

        if (!ViewBlackListInstances.ContainsKey(component.Message.Id))
        {
            await component.FollowupAsync("This interaction expired. Worships list interactions expire after 15 minutes, run `/blacklist view` again.", ephemeral: true);
            return;
        }
        
        ViewBlackListInstance thisInstance = ViewBlackListInstances[component.Message.Id];
        
        if (component.User.Id != thisInstance.User.Id)
        {
            await component.FollowupAsync("This interaction is not yours, run `/blacklist view` yourself.", ephemeral: true);
            return;
        }

        _ = component.Data.CustomId == "BlackListBack" ? thisInstance.CurrentIndex -= 5 : thisInstance.CurrentIndex += 5;
        
        await component.Message.ModifyAsync(m =>
        {
            ComponentBuilder instanceComponentBuilder = new ComponentBuilder();
            if (thisInstance.CurrentIndex != 0) instanceComponentBuilder = instanceComponentBuilder.WithButton(BackButton);
            if (thisInstance.CurrentIndex + 5 < _blackList.Count) instanceComponentBuilder = instanceComponentBuilder.WithButton(ForwardButton);
                
            m.Embed = GetPageEmbed(thisInstance.CurrentIndex, thisInstance.EmbedColor).Build();
            m.Components = instanceComponentBuilder.Build();
        });

        ViewBlackListInstances[component.Message.Id] = thisInstance;
    }
    
    [SlashCommand("view", "View blacklisted users"), UsedImplicitly,
     CommandCooldown(15), OnlyModerators(ModeratorsSelection.AllMods, "❌ Only moderators can view blacklisted users due to privacy reasons ❌")]
    public async Task ViewBlackListAsync()
    {
        await DeferAsync();

        _blackList = DataBase.RunSqliteCommandAllRows($"SELECT Id FROM BlackListedUsers");

        if (_blackList.Count == 0)
        {
            EmbedBuilder noWorshipsEmbed = new EmbedBuilder()
                .WithTitle("No blacklisted users")
                .WithDescription($"There is no blacklisted users, blacklist someone and it will be shown here.")
                .WithColor(RandomColor());

            await ModifyOriginalResponseAsync(r => r.Embed = noWorshipsEmbed.Build());
            return;
        }

        uint embedColor = RandomColor();

        await ModifyOriginalResponseAsync(r =>
        {
            r.Embed = GetPageEmbed(0, embedColor).Build();
            r.Components = _blackList.Count <= 5 ? null : new ComponentBuilder().WithButton(ForwardButton).Build();
        });
        
        if(_blackList.Count <= 5) return;

        ulong interactionMessageId = (await Context.Interaction.GetOriginalResponseAsync()).Id;
        
        ViewBlackListInstances.Add(interactionMessageId, new ViewBlackListInstance{User = Context.User, CurrentIndex = 0, EmbedColor = embedColor});

        async void RemoveInstanceThreadHandler(ulong id)
        {
            await Task.Delay(TimeSpan.FromMinutes(15));
            ViewBlackListInstances.Remove(id);
        }
        
        new Thread(() => RemoveInstanceThreadHandler(interactionMessageId)).Start();
    }
}