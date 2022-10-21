﻿using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;

namespace JamieWorshipper.Commands;

[Group("worships", "Worships related command group.")]
public class Worships : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("view", "View all of the worships sent to jamie."), CommandCooldown(60), UsedImplicitly]
    public async Task ViewAsync([Summary("Order", "The order which the worships will be displayed"), Choice("Ascending", "ASC"), Choice("Descending", "DESC")]string order = "ASC")
    {
        await DeferAsync();
        
        ButtonBuilder backButton = new ButtonBuilder()
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Back")
            .WithEmote(new Emoji("⬅"))
            .WithCustomId("Back");
        
        ButtonBuilder forwardButton = new ButtonBuilder()
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Forward")
            .WithEmote(new Emoji("➡"))
            .WithCustomId("Forward");

        List<List<object>> worshipData = DataBase.RunSqliteCommandAllRows($"SELECT UserId, Worship, Guild, Id FROM Worships ORDER BY Id {order}");

        if (worshipData.Count == 0)
        {
            EmbedBuilder noWorshipsEmbed = new EmbedBuilder()
                .WithTitle("No worships :(")
                .WithDescription("Sadly jamie has no worshippers yet, Use `/worships do` to worship jamie")
                .WithColor(RandomColor());

            await ModifyOriginalResponseAsync(r => r.Embed = noWorshipsEmbed.Build());
            return;
        }

        EmbedBuilder GetPageEmbed(int start) => new EmbedBuilder()
            .WithTitle("Here are all the worshipers")
            .WithFields(worshipData.Skip(start).Take(5).Select(async s => new EmbedFieldBuilder()
                .WithName($"User: {await Client.GetUserAsync((ulong)(long)s[0])}")
                .WithValue($"**ID:** {s[3]}\n**Worship:** {s[1]}\n**Guild:** {(string)s[2]}")
            ).Select(r => r.Result))
            .WithColor(RandomColor())
            .WithFooter($"Showing {start + 1}-{start + 6} out of {worshipData.Count}");
        
        await ModifyOriginalResponseAsync(r =>
        {
            r.Embed = GetPageEmbed(0).Build();
            r.Components = worshipData.Count <= 5 ? null : new ComponentBuilder().WithButton(forwardButton).Build();
        });
        
        if(worshipData.Count <= 5) return;

        int currentIndex = 0;

        Client.ButtonExecuted += async component =>
        {
            await component.DeferAsync(ephemeral: true);
            
            if (component.User.Id != Context.User.Id)
            {
                await component.ModifyOriginalResponseAsync(r => r.Content = "This interaction is not yours, run `/worships view` yourself.");
                return;
            }

            _ = component.Data.CustomId == "Back" ? currentIndex -= 5 : currentIndex += 5;
            await component.Message.ModifyAsync(m =>
            {
                ComponentBuilder instanceComponentBuilder = new ComponentBuilder();
                if (currentIndex != 0) instanceComponentBuilder = instanceComponentBuilder.WithButton(backButton);
                if (currentIndex + 5 < worshipData.Count) instanceComponentBuilder = instanceComponentBuilder.WithButton(forwardButton);
                
                m.Embed = GetPageEmbed(currentIndex).Build();
                m.Components = instanceComponentBuilder.Build();
            });
        };
    }

    [SlashCommand("do", "Worship Jamie!"), CommandCooldown(30), UsedImplicitly]
    public async Task DoAsync([Summary("Worship", "The worship paragraph"), MaxLength(300)]string worship)
    {
        await DeferAsync();
        
        if (Context.User.Id == Config.Jamie.Id)
        {
            await FollowupAsync("🚫 I know you are worthy but you can't worship yourself. 🚫", ephemeral: true);
            await DeleteOriginalResponseAsync();
            return;
        }
        
        List<List<object>> blackList = DataBase.RunSqliteCommandAllRows($"SELECT Id FROM BlackListedUsers WHERE Id = {Context.User.Id}");
        if (blackList.Count > 0)
        {
            await FollowupAsync("🚫 You are in the blacklist, you cannot worship jamie. 🙅", ephemeral: true);
            await DeleteOriginalResponseAsync();
            return;
        }

        worship = string.Join(" ", Regex.Replace(worship, "[*`]", "")
            .Split(" ")
            .Select(w => w.Contains("https://cdn.discordapp.com") || w.Contains("https://tenor.com") ? $"[[click to view attachment]]({w})" : w));

        try
        {
            string[] phrases = {"You got worshipped my lord", "Someone worshipped you", "They are glad you exist", "Hello god, I got a worship"};
            List<object> worshipCount = DataBase.RunSqliteCommandFirstRow("SELECT Id + 1 FROM Worships ORDER BY Id DESC");
            
            EmbedBuilder sentWorshipEmbed = new EmbedBuilder()
                .WithTitle(phrases[new Random().Next(0, phrases.Length - 1)])
                .AddField($"{Context.User} said", worship)
                .WithFooter($"The id of this worship is {(long)worshipCount[0]}");
            
            await Config.Jamie.SendMessageAsync(embed: sentWorshipEmbed.Build());

            DataBase.RunSqliteCommandAllRows($"INSERT INTO Worships(UserId, Worship, Guild, Id) VALUES({Context.User.Id}, @0, @1, null)", DatabaseHandler.ParseInput(worship), Context.Interaction.IsDMInteraction ? "Private messages" : DatabaseHandler.ParseInput(Context.Guild.Name));
            BotStatsHandler.WorshipsNum++;
            
            EmbedBuilder worshipEmbed = new EmbedBuilder()
                .WithTitle("Worship sent")
                .WithDescription($"Your worship was sent successfully\n**you sent:** {worship}")
                .WithFooter("You can check other's worships using /worships view")
                .WithColor(0x00ff00);
            
            await ModifyOriginalResponseAsync(r => r.Embed = worshipEmbed.Build());
        }
        catch
        {
            EmbedBuilder failedEmbed = new EmbedBuilder()
                .WithTitle("Worship failed to send")
                .WithDescription("Your worship failed because one of the following reasons\n- Jamie blocked the bot\n- Memw is a dumb ass and can't code properly")
                .WithFooter("You can retry, you don't lose anything.")
                .WithColor(0xff0000);

            await ModifyOriginalResponseAsync(r => r.Embed = failedEmbed.Build());
        }
    }

    [SlashCommand("reply", "Reply to some worship."), UsedImplicitly]
    public async Task ReplyAsync([Summary("ID", "Some worship ID.")]int id, [Summary("Reply", "What do you want to reply with?"), MaxLength(300)]string reply)
    {
        await DeferAsync(ephemeral: true);
        
        if (Context.User.Id != Config.Jamie.Id)
        {
            await ModifyOriginalResponseAsync(r => r.Content = "🚫 Only Jamie can reply to worships, 👀 you don't look like Jamie!?");
            return;
        }

        List<object> worship = DataBase.RunSqliteCommandFirstRow($"SELECT UserId, Worship, Guild, Id FROM Worships WHERE Id = {id}");

        if (worship.Count == 0)
        {
            await ModifyOriginalResponseAsync(r => r.Content = "Looks like a worship with this ID does not exist, use `/worships view` to view all the worships and them ID's");
            return;
        }

        try
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("Jamie replied to your worship")
                .WithDescription($"**He said:** {reply}")
                .AddField("Worship info", $"**ID:** {id}\n**Content:** {worship[1]}")
                .WithColor(RandomColor())
                .WithCurrentTimestamp();
            
            IUser user = await Client.GetUserAsync((ulong)(long)worship[0]);
            await user.SendMessageAsync(embed: embed.Build());
            await ModifyOriginalResponseAsync(r => r.Content = $"Your reply was sent to `{user}`");
        }
        catch
        {
            await ModifyOriginalResponseAsync(r => r.Content = "There is no communication with the user you want to reply (The reply was not sent), you might want to reply them yourself.");
        }
    }

    [SlashCommand("leaderboard", "Check the users who most worshipped jamie."), CommandCooldown(15), UsedImplicitly]
    public async Task LeaderBoardAsync()
    {
        await DeferAsync();
        List<List<object>> worships = DataBase.RunSqliteCommandAllRows("SELECT UserId FROM Worships");
        Dictionary<ulong, int> worshipsCount = new();
        foreach (var user in worships)
            if(worshipsCount.ContainsKey((ulong)(long)user[0])) worshipsCount[(ulong)(long)user[0]]++; 
            else worshipsCount.Add((ulong)(long)user[0], 1);
        Dictionary<ulong, int> topFive = worshipsCount.OrderByDescending(w => w.Value)
            .Take(5).ToDictionary(v => v.Key, v => v.Value);
        List<KeyValuePair<ulong, int>> thisUserOrdered = worshipsCount.OrderByDescending(w => w.Value).CreateOrderedEnumerable(w => w.Value, null, true).ToList();
        int thisUserPos = thisUserOrdered.FindIndex(w => w.Key == Context.User.Id);
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("🏆 Leaderboard 🏆")
            .WithDescription("🙏 these are the top 5 worshipers who worshiped the most 🙏")
            .WithFooter(thisUserPos != -1 ? $"You are {(thisUserPos < 3 ? $"{thisUserPos}st" : $"{thisUserPos}th")} with {thisUserOrdered.First(u => u.Key == Context.User.Id).Value} worships in a descending list of all the worshippers." : "You don't have any worship yet, use \"/worships do\" to worship jamie")
            .WithColor(RandomColor())
            .WithFields(topFive.Select(async (w, i) => new EmbedFieldBuilder().WithName($"{i + 1}{((i + 1) % 10) switch {1 => "st", 2 => "nd", 3 => "rd", _ => "th"}} {await Client.GetUserAsync(w.Key)}").WithValue($"**With:** {w.Value} worships")).Select(t => t.Result));
        await ModifyOriginalResponseAsync(r => r.Embed = embed.Build());
    }
}