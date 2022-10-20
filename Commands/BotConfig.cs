using Discord.Interactions;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;    

namespace JamieWorshipper.Commands;

public class BotConfig : InteractionModuleBase<SocketInteractionContext>
{
    //[SlashCommand("config", "Lets the moderators change the global bot config."), UsedImplicitly]
    public async Task ChangeConfigAsync()
    {
        await DeferAsync(ephemeral: true);
        
        if (!Config.Moderators.Contains(Context.User.Id))
        {
            await ModifyOriginalResponseAsync(r => r.Content = "❌ You cannot change the bot global configuration as you are not a bot moderator ❌");
            return;
        }

        ConfigHandler savedConfig = Config;
        ChangeConfigResult res = await Config.ReloadConfig();

        if (res.Exception != null)
        {
            await ModifyOriginalResponseAsync(r => r.Content = $"There was an error with your new config: {res.Exception.Message}");
        }
        else
        {
            await ModifyOriginalResponseAsync(r => r.Content = "Your configuration values were changed successfully, the new configuration values were:");
        }
    }    
}