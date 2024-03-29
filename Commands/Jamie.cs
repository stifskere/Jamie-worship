﻿using Discord;
using Discord.Interactions;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;

namespace JamieWorshipper.Commands;

[Group("jamie", "Jamie related commands group.")]
public class Jamie : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("profile", "View jamie profile."), CommandCooldown(15), UsedImplicitly]
    public async Task JamieProfileAsync()
    {
        await DeferAsync();
        
        IRole closeFriendsRole = Config.MainGuild.Roles.First(r => r.Id == Config.CloseFriendsRole.Id);
        List<List<object>> dbData = DataBase.RunSqliteCommandAllRows(@"
        SELECT count(*) FROM Worships UNION ALL
        SELECT InfoValue FROM JamieInfo WHERE InfoKey = 'MessageNum' UNION ALL
        SELECT InfoValue FROM JamieInfo WHERE InfoKey = 'LastMessage';        
        ");

        string closeFriends = (await Config.MainGuild.GetUsersAsync())
            .Where(u => u.RoleIds.Any(s => s == closeFriendsRole.Id))
            .Aggregate("", (current, user) => current + $"{user}, ")[..^2];

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Jamie")
            .WithDescription($"**Jamie's tag:** {Config.Jamie}\n**Jamie's id:** `{Config.Jamie.Id}`\n\nHe lives in UK even tho he says he doesn't")
            .WithFields(
                new EmbedFieldBuilder().WithName("🔹 Jamie's close friends").WithValue(closeFriends),
                new EmbedFieldBuilder().WithName("🔹 Jamie's bot stats").WithValue($"**Worships:** {dbData[0][0]}\n**Counted messages:** {dbData[1][0]}\n**Last message:** {dbData[2][0]}")
            )
            .WithThumbnailUrl(Config.Jamie.GetAvatarUrl())
            .WithColor(RandomColor());

        await ModifyOriginalResponseAsync(r => r.Embed = embed.Build());
    }

    [SlashCommand("gif", "Jamie gifgifgifgif."), CommandCooldown(30), RequireBotPermission(ChannelPermission.AttachFiles), UsedImplicitly]
    public async Task JamiePhotoAsync([Summary("Send", "Indicate whether send the gif to jamie or not.")]bool send = false)
    {
        await DeferAsync();

        if (!File.Exists("./build/JamieGif.gif"))
        {
            async Task CreateFileTask()
            {
                await using FileStream fs = File.Create("./build/JamieGif.gif");
                using HttpClient client = new HttpClient();
                await fs.WriteAsync(await client.GetByteArrayAsync("https://cdn.memw.es/helloJamie.gif"));
            }

            await CreateFileTask();
        }

        await Context.Channel.SendFileAsync(filePath: "./build/JamieGif.gif");
        await ModifyOriginalResponseAsync(r => r.Content = "Loaded.");
        if (send)
            try
            {
                if (Context.User.IsOnBlackList())
                {
                    await FollowupAsync("🙅 You are in the blacklist, you cannot send anything to jamie. 🚫", ephemeral: true);
                    return;
                }

                await Config.Jamie.SendFileAsync("./build/JamieGif.gif");
                await FollowupAsync("The gif was sent to jamie", ephemeral: true);
            }
            catch
            {
                await FollowupAsync("An error occurred, the gif was not sent, maybe jamie blocked the bot, but you can still enjoy your gif without sending it.", ephemeral: true);
            }

        await DeleteOriginalResponseAsync();
    }
}