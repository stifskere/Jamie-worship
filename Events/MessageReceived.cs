using Discord;
using Discord.WebSocket;
using JamieWorshipper.Handlers;
using JetBrains.Annotations;

namespace JamieWorshipper.Events;

public static class MessageReceived
{
    [Event(EventTypes.MessageReceived), UsedImplicitly]
    public static Task MessageReceivedEvent(SocketMessage message)
    {
        new Thread(() => MessageReceivedTask(message)).Start();
        return Task.CompletedTask;
    }

    public static async void MessageReceivedTask(SocketMessage message)
    {
        if (message.Author.IsBot || message.Channel is IDMChannel) return;
        
        IGuild messageGuild = (message.Channel as SocketGuildChannel)?.Guild ?? Client.Guilds.First(m => m.Channels.Any(c => c.Id == message.Channel.Id));

        if (message.Author.Id == Config.Jamie.Id && !string.IsNullOrEmpty(message.Content))
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("Jamie said something")
                .WithDescription($"**He said:** {message.Content}\n**In:** {messageGuild.Name}")
                .WithFooter("I'l keep whatever our god says in here");

            ComponentBuilder components = new ComponentBuilder()
                .WithButton(new ButtonBuilder { Label = "Go to message", Style = ButtonStyle.Link, Url = message.GetJumpUrl() });

            await Config.MessagesChannel.SendMessageAsync(embed: embed.Build(), components: components.Build());

            DataBase.RunSqliteCommandAllRows(@"
            UPDATE JamieInfo SET InfoValue = CAST((SELECT CAST(InfoValue AS INTEGER) FROM JamieInfo WHERE InfoKey = 'MessageNum') + 1 AS TEXT) WHERE InfoKey = 'MessageNum';
            UPDATE JamieInfo SET InfoValue = @0 WHERE InfoKey = 'LastMessage';
            ", message.Content);
        }

        if (message.Content.ToLower().Contains("jamie") && messageGuild.Id == Config.MainGuild.Id && !_jamieMessagesCooldown)
        {
            string[] messages = { "Want to talk about Jamie with me?", "Did you say Jamie?", "I also like Jamie", "Jamie is a god for me", "You talking about jamie and not telling me!?" };
            await message.Channel.SendMessageAsync(messages[new Random().Next(0, 4)], messageReference: new MessageReference(message.Id));
            
            new Thread(() =>
            {
                _jamieMessagesCooldown = true;
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _jamieMessagesCooldown = false;
            }).Start();
        }
    }

    private static bool _jamieMessagesCooldown;
}