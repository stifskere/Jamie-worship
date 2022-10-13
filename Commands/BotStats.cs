using System.Net.Http.Headers;
using System.Text;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static JamieWorshiper.Handlers.BotStatsHandler;

namespace JamieWorshiper.Commands;

[Group("bot", "Bot stats related command group.")]
public class BotStats : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("stats", "View the actual bot status since start-up."), UsedImplicitly]
    public async Task BotStatsAsync()
    {
        string usedCommandsString = "  Command Name  |  Count\n--------------------------\n";

        if (CommandCount > 0)
            foreach (KeyValuePair<string, int> entry in CommandUsage)
            {
                string countAddition = ".........................";
                countAddition = $"{entry.Key}{countAddition[entry.Key.Length..]}";
                countAddition = $"{countAddition[..^entry.Value.ToString().Length]}{entry.Value.ToString()}";
                usedCommandsString += $"{countAddition}\n";
            }
        else usedCommandsString = "No commands used yet.";

        long seconds = DateTimeOffset.Now.ToUnixTimeSeconds() - Uptime.ToUnixTimeSeconds(), minutes = seconds / 60, hours = minutes / 60, days = hours / 24,
            fSeconds = (long)Math.Floor((decimal)seconds) % 60, fMinutes = (long)Math.Floor((decimal)minutes) % 60, fHours = (long)Math.Floor((decimal)hours), fDays = (long)Math.Floor((decimal)days);
        string parsedTimeString = $"{(fDays != 0 ? $"{(fDays < 10 ? $"0{fDays}" : fDays)} day{(fDays == 1 ? "" : "s")}" : "")} {(fHours != 0 ? $"{(fHours < 10 ? $"0{fHours}" : fHours)} hour{(fHours == 1 ? "" : "s")}" : "")} {(fMinutes != 0 ? $"{(fMinutes < 10 ? $"0{fMinutes}" : fMinutes)} minute{(fMinutes == 1 ? "" : "s")}" : "")} {(fSeconds != 0 ? $"{(fSeconds < 10 ? $"0{fSeconds}" : fSeconds)} second{(fSeconds == 1 ? "" : "s")}" : "")}";

        dynamic[] commits = Array.Empty<dynamic>();
        using HttpClient requestClient = new();
        requestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(Client.Env["GITCREDS"])));
        requestClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("JamieWorshipper", "1.0"));
        requestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        for (float i = 1; i < float.PositiveInfinity; i++)
        {
            JArray gitObj = (JArray)JsonConvert.DeserializeObject(await requestClient.GetStringAsync($"https://api.github.com/repos/stifskere/Jamie-worship/commits?per_page=100&page={i}"))!;
            commits = commits.Concat(gitObj.ToObject<dynamic[]>()!).ToArray();
            if(gitObj.Count != 100) break;
        }

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Bot stats")
            .WithDescription("This shows the current bot stats since the last bot restart.")
            .WithFields(
                new EmbedFieldBuilder().WithName("🔹 General stats").WithValue($"**Current up time:** {parsedTimeString}\n**Worships sent:** {WorshipsNum}\n**Total commands used:** {CommandCount}"),
                new EmbedFieldBuilder().WithName("🔹 Detailed command usage").WithValue($"```\n{usedCommandsString}```"),
                new EmbedFieldBuilder().WithName("🔹 GitHub status").WithValue($"**Commits:** {commits.Length}\n**Last commit name:** {commits[0].commit.message}\n**Last commit author:** {commits[0].commit.author.name}\n**Last commit content:** [click to view changes]({commits[0].html_url})")
                )
            .WithColor(RandomColor())
            .WithFooter("Report any errors to Memw#6969")
            .WithCurrentTimestamp();

        await RespondAsync(embed: embed.Build());
    }
}