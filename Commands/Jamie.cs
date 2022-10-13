using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace JamieWorshipper.Commands;

[Group("jamie", "Jamie related commands group.")]
public class Jamie : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("profile", "View jamie profile."), UsedImplicitly]
    public async Task JamieProfileAsync()
    {
        IRole closeFriendsRole = Config.MainGuild.Roles.First(r => r.Id == 976175903371571220);
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

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("gif", "Jamie gifgifgifgif."), RequireBotPermission(ChannelPermission.AttachFiles), UsedImplicitly]
    public async Task JamiePhotoAsync([Summary("Send", "Indicate whether send the gif to jamie or not.")]bool send)
    {
        HttpClient requestClient = new HttpClient();
        using MemoryStream image = new MemoryStream(await requestClient.GetByteArrayAsync("https://cdn.memw.es/helloJamie.gif"));
        await RespondWithFileAsync(image, "jamie.gif");
        if (send) try 
            {
               await Config.Jamie.SendFileAsync(image, "jamie.gif");
            }
            catch {/* ignored */}
    }
}