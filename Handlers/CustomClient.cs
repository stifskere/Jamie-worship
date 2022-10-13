using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace JamieWorshipper.Handlers;

public class CustomClient : DiscordSocketClient
{
    public readonly EnvReader Env;

    public bool RunOnce { get; internal set; } = true;

    public class EnvReader
    {
        private readonly Dictionary<string, string> _variables;
        public string this[string key] => _variables[key];
        internal EnvReader(Dictionary<string, string> dictionary) {_variables = dictionary;}
    }

    public CustomClient(DiscordSocketConfig baseConfig, CustomClientConfig customConfig) : base(baseConfig)
    {
        if (!string.IsNullOrEmpty(customConfig.EnvPath))
        {
            string dir = string.Join("/", Regex.Split(customConfig.EnvPath, @"[/\\]", RegexOptions.None)[..^1]);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            Env = new EnvReader(File.ReadAllLines(customConfig.EnvPath).ToDictionary(envVars => envVars.Split('=')[0], envVars => envVars.Split('=')[1]));
        }
        else Env = new EnvReader(new Dictionary<string, string>());
        
        foreach (MethodInfo method in customConfig.Assembly.GetTypes().SelectMany(t => t.GetMethods()).Where(m => m.GetCustomAttributes(typeof(EventAttribute), false).Length > 0).ToArray())
        {
            CustomAttributeData customAttributeData = method.GetCustomAttributesData().First(a => a.AttributeType.Name == "EventAttribute");
            EventInfo eventData = GetType().GetEvent(((EventTypes)customAttributeData.ConstructorArguments.First().Value!).ToString())!;
            eventData.AddEventHandler(this, method.CreateDelegate(eventData.EventHandlerType!));
        }
        
        Disconnected += OnDisconnected;
        Log += OnLog;
    }

    private Task OnLog(LogMessage message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }

    private Task OnDisconnected(Exception exception)
    {
        RunOnce = false;
        return Task.CompletedTask;
    }
}

[PublicAPI]
public struct CustomClientConfig
{
    public string EnvPath { get; init; } = "";
    public Assembly Assembly { get; init; } = Assembly.GetExecutingAssembly();
    public CustomClientConfig(){}
}

[AttributeUsage(AttributeTargets.Method)]
public class EventAttribute : Attribute
{
    [UsedImplicitly]
    public EventAttribute(EventTypes ev){}
}

[PublicAPI]
public enum EventTypes
{
    Connected,
    Disconnected,
    Ready,
    AutocompleteExecuted,
    ButtonExecuted,
    ChannelCreated,
    ChannelDestroyed,
    ChannelUpdated,
    GuildAvailable,
    GuildUnavailable,
    GuildUpdated,
    IntegrationCreated,
    IntegrationDeleted,
    IntegrationUpdated,
    InteractionCreated,
    InviteCreated,
    InviteDeleted,
    JoinedGuild,
    LatencyUpdated,
    LeftGuild,
    LoggedIn,
    LoggedOut,
    MessageDeleted,
    MessageReceived,
    MessageUpdated,
    ModalSubmitted,
    PresenceUpdated,
    ReactionAdded,
    ReactionRemoved,
    ReactionsCleared,
    RecipientAdded,
    RecipientRemoved,
    RoleCreated,
    RoleDeleted,
    RoleUpdated,
    SpeakerAdded,
    SpeakerRemoved,
    StageEnded,
    StageStarted,
    StageUpdated,
    ThreadCreated,
    ThreadDeleted,
    ThreadUpdated,
    UserBanned,
    UserJoined,
    UserLeft,
    UserUnbanned,
    UserUpdated,
    WebhooksUpdated,
    ApplicationCommandCreated,
    ApplicationCommandDeleted,
    ApplicationCommandUpdated,
    CurrentUserUpdated,
    GuildMembersDownloaded,
    GuildMemberUpdated,
    GuildStickerCreated,
    GuildStickerDeleted,
    GuildStickerUpdated,
    MessageCommandExecuted,
    MessageBulkDeleted,
    RequestToSpeak,
    SelectMenuExecuted,
    SlashCommandExecuted,
    ThreadMemberJoined,
    ThreadMemberLeft,
    UserCommandExecuted,
    UserIsTyping,
    VoiceServerUpdated,
    GuildJoinRequestDeleted,
    GuildScheduledEventCancelled,
    GuildScheduledEventCompleted,
    GuildScheduledEventCreated,
    GuildScheduledEventStarted,
    GuildScheduledEventUpdated,
    ReactionsRemovedForEmote,
    UserVoiceStateUpdated,
    GuildScheduledEventUserAdd,
    GuildScheduledEventUserRemove,
    CustomLogEvent
}