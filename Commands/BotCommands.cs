using System.Net.Http.Headers;
using System.Text;
using Discord;
using Discord.Interactions;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static JamieWorshipper.Handlers.BotStatsHandler;

namespace JamieWorshipper.Commands;

[Group("bot", "Bot commands related command group.")]
public class BotCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("stats", "View the actual bot status since start-up."), CommandCooldown(15), UsedImplicitly]
    public async Task BotStatsAsync()
    {
        await DeferAsync();
        
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
            fSeconds = (long)Math.Floor((decimal)seconds) % 60, fMinutes = (long)Math.Floor((decimal)minutes) % 60, fHours = (long)Math.Floor((decimal)hours) % 24, fDays = (long)Math.Floor((decimal)days);
        string parsedTimeString = $"{(fDays != 0 ? $"{(fDays < 10 ? $"0{fDays}" : fDays)} day{(fDays == 1 ? "" : "s")} " : "")}{(fHours != 0 ? $"{(fHours < 10 ? $"0{fHours}" : fHours)} hour{(fHours == 1 ? "" : "s")} " : "")}{(fMinutes != 0 ? $"{(fMinutes < 10 ? $"0{fMinutes}" : fMinutes)} minute{(fMinutes == 1 ? "" : "s")} " : "")}{(fSeconds != 0 ? $"{(fSeconds < 10 ? $"0{fSeconds}" : fSeconds)} second{(fSeconds == 1 ? "" : "s")}" : "")}";

        async Task<dynamic[]> FetchGithubData()
        {
            dynamic[] commits = Array.Empty<dynamic>();
            
            using HttpClient requestClient = new();
            requestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(Client.Env["GITCREDS"])));
            requestClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("JamieWorshipper", "1.0"));
            requestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            for (int i = 0;;i++)
            {
                JArray gitObj = (JArray)JsonConvert.DeserializeObject(await requestClient.GetStringAsync($"https://api.github.com/repos/stifskere/Jamie-worship/commits?per_page=100&page={i}"))!;
                commits = commits.Concat(gitObj.ToObject<dynamic[]>()!).ToArray();
                if(gitObj.Count != 100) break;
            }

            return commits;
        }
        
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Bot stats")
            .WithDescription("This shows the current bot stats since the last bot restart.")
            .WithFields(
                new EmbedFieldBuilder().WithName("🔹 General stats").WithValue($"**Current up time:** {parsedTimeString}\n**Worships sent:** {WorshipsNum}\n**Total commands used:** {CommandCount}"),
                new EmbedFieldBuilder().WithName("🔹 Detailed command usage").WithValue($"```\n{usedCommandsString}```"),
                new EmbedFieldBuilder().WithName("🔹 GitHub status").WithValue("loading...")
                )
            .WithColor(RandomColor())
            .WithFooter("Report any errors to Memw#6969")
            .WithCurrentTimestamp();

        await ModifyOriginalResponseAsync(r => r.Embed = embed.Build());

        dynamic[] commits = await FetchGithubData();
        embed.Fields[2].Value = $"**Commits:** {commits.Length}\n**Last commit name:** {commits[0].commit.message}\n**Last commit author:** {commits[0].commit.author.name}\n**Last commit content:** [click to view changes]({commits[0].html_url})";
        
        await ModifyOriginalResponseAsync(r => r.Embed = embed.Build());
    }
    
    [SlashCommand("config", "Lets the moderators change the global bot config."), UsedImplicitly, OnlyModerators(ModeratorsSelection.AllMods, "❌ You cannot change the bot global configuration as you are not a bot moderator ❌")]
    public async Task ChangeConfigAsync(
        [Summary("Key", "Config parameter want to be set"),
         Choice("Jamie messages channel", "MessagesChannel"),
         Choice("Main guild", "MainGuild"),
         Choice("Jamie ID", "JamieId"),
         Choice("Close friends role", "CloseFriendsRole")]string key, 
        [Summary("Value", "The value want to be set to such config parameter")]string value)
    {
        await DeferAsync(ephemeral: true);

        List<object> valueSave = DataBase.RunSqliteCommandFirstRow($"SELECT ConfigValue FROM BotConfig WHERE ConfigKey = '{key}'");
        DataBase.RunSqliteCommandAllRows($"UPDATE BotConfig SET ConfigValue = '{value}' WHERE ConfigKey = '{key}'");
        
        Exception? res = await Config.ReloadConfig();
        if (res != null)
        {
            DataBase.RunSqliteCommandAllRows($"UPDATE BotConfig SET ConfigValue = '{valueSave[0]}' WHERE ConfigKey = '{key}'");
            await Config.ReloadConfig();

            EmbedBuilder errorEmbed = new EmbedBuilder()
                .WithTitle("There was an error with your config.")
                .WithDescription($"The error says the following\n```\n{(res is NullReferenceException ? "The bot can't access such item ID" : res.Message)}\n```")
                .WithCurrentTimestamp()
                .WithColor(0xff0000);
            
            await ModifyOriginalResponseAsync(r => r.Embed = errorEmbed.Build());
            return;
        }

        EmbedBuilder successEmbed = new EmbedBuilder()
            .WithTitle("The config was changed successfully")
            .AddField("Values", $"```diff\n- {key}: {valueSave[0]}\n+ {key}: {value}\n```")
            .WithCurrentTimestamp()
            .WithColor(0x00ff00);

        await ModifyOriginalResponseAsync(r => r.Embed = successEmbed.Build());
    }
}