using Discord;
using Discord.Interactions;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;    

namespace JamieWorshipper.Commands;

public class BotConfig : InteractionModuleBase<SocketInteractionContext>
{
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