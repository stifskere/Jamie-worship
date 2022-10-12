using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace JamieWorshiper.Commands;

[Group("jamie", "Jamie related commands group")]
public class Jamie : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("profile", "View jamie info"), UsedImplicitly]
    public async Task JamieProfileAsync()
    {
        IRole closeFriendsRole = Config.MainGuild.Roles.First(r => r.Id == 976175903371571220);
        List<List<object>> dbData = DataBase.RunSqliteCommandAllRows(@"
        SELECT count(*) FROM Worshippers;
        SELECT InfoValue, InfoKey FROM JamieInfo WHERE InfoKey = 'MessageNum';
        SELECT InfoKey, InfoKey FROM JamieInfo WHERE InfoKey = 'LastMessage';        
        ");

        string closeFriends = (await Config.MainGuild.GetUsersAsync())
            .Select(u => u.RoleIds.Any(s => s == closeFriendsRole.Id))
            .Aggregate("", (current, user) => current + $"{user}, ")[..^1];

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Jamie")
            .WithDescription($"**Jamie's tag:** {Config.Jamie}\n**Jamie's id:** `{Config.Jamie.Id}`\n\nHe lives in UK even tho he says he doesn't")
            .WithFields(
                new EmbedFieldBuilder().WithName("🔹 Jamie's close friends").WithValue(closeFriends),
                new EmbedFieldBuilder().WithName("🔹 Jamie's bot stats").WithValue($"**Worships:**\n**Counted messages:**\n**Last message:**")
            )
            .WithThumbnailUrl(Config.Jamie.GetAvatarUrl())
            .WithColor(RandomColor());

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("gif", "Jamie gifgifgifgif"), UsedImplicitly]
    public async Task JamiePhotoAsync()
    {
        HttpClient requestClient = new HttpClient();
        using MemoryStream image = new MemoryStream(await requestClient.GetByteArrayAsync("https://cdn.memw.es/helloJamie.gif"));
        await RespondWithFileAsync(image, "jamie.gif");
    }
}