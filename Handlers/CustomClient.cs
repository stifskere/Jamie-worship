﻿using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using PreconditionAttribute = Discord.Interactions.PreconditionAttribute;
using PreconditionResult = Discord.Interactions.PreconditionResult;

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
        
        foreach (MethodInfo method in customConfig.Assembly.GetTypes().SelectMany(t => t.GetMethods()).Where(m => m.GetCustomAttributes(typeof(EventAttribute), false).Length > 0))
        {
            EventInfo eventData = GetType().GetEvent(((EventTypes)method.GetCustomAttributesData().First(a => a.AttributeType.Name == "EventAttribute").ConstructorArguments.First().Value!).ToString())!;
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

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CommandCooldownAttribute : PreconditionAttribute
{
    private static readonly Dictionary<ulong, List<Command>> CooldownList = new();
    private short Seconds { get; }
    public CommandCooldownAttribute(short secs)
    {
        Seconds = secs;
    }
    
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        string cName;
        if (context.Interaction.Data is SocketSlashCommandData slashData)
            cName = $"{slashData.Name} {string.Join(" ", slashData.Options.Select(option => option.Name))}";
        else
        {
            SocketUserCommandData userData = (context.Interaction.Data as SocketUserCommandData)!;
            cName = $"{userData.Name}";
        }

        if (!CooldownList.ContainsKey(context.User.Id))
        {
            CooldownList.Add(context.User.Id, new List<Command>
            {
                new()
                {
                   Name = cName,
                   Timestamp = DateTimeOffset.UtcNow.AddSeconds(Seconds)
                }
            });
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        Command? command = CooldownList[context.User.Id].FirstOrDefault(c => c.Name == cName);
        
        if (command != null)
        {
            Command commandValue = command.Value;
            if (commandValue.Timestamp.ToUnixTimeSeconds() < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                CooldownList[context.User.Id].RemoveAll(c => c.Name == cName);
            else
            {
                if (BotStatsHandler.CommandUsage.ContainsKey(cName))
                {
                    BotStatsHandler.CommandCount--;
                    BotStatsHandler.CommandUsage[cName]--;
                }
                
                async void CoolDownMessageThread()
                {
                    long time = commandValue.Timestamp.ToUnixTimeMilliseconds() - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (time < 1000) time += 1000;
                    await context.Interaction.RespondAsync($"This command is in cooldown, you will be able to use it <t:{commandValue.Timestamp.ToUnixTimeSeconds()}:R>", ephemeral: true);
                    await Task.Delay((int)time);
                    await context.Interaction.ModifyOriginalResponseAsync(m => m.Content = "You are now able to use the command again.");
                }
                
                new Thread(CoolDownMessageThread).Start();
                return Task.FromResult(PreconditionResult.FromError(string.Empty));
            }
        }
        
        if (CooldownList[context.User.Id].All(c => c.Name != cName))
        {
            CooldownList[context.User.Id].Add(new Command
            {
                Name = cName,
                Timestamp = DateTimeOffset.UtcNow.AddSeconds(Seconds)
            });
        }
        return Task.FromResult(PreconditionResult.FromSuccess());
    }

    private struct Command
    {
        internal string Name { get; init; }
        internal DateTimeOffset Timestamp { get; init; }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class OnlyModeratorsAttribute : PreconditionAttribute
{
    private ModeratorsSelection ModeratorsSelection { get; }
    private string ErrorString { get; }

    public OnlyModeratorsAttribute(ModeratorsSelection selection, string errorString)
    {
        ModeratorsSelection = selection;
        ErrorString = errorString;
    }

    private static void RemoveFromStats(IInteractionContext context)
    {
        string cName;
        if (context.Interaction.Data is SocketSlashCommandData slashData)
            cName = $"{slashData.Name} {string.Join(" ", slashData.Options.Select(option => option.Name))}";
        else
        {
            SocketUserCommandData userData = (context.Interaction.Data as SocketUserCommandData)!;
            cName = $"{userData.Name}";
        }
            
        if (BotStatsHandler.CommandUsage.ContainsKey(cName))
        {
            BotStatsHandler.CommandCount--;
            BotStatsHandler.CommandUsage[cName]--;
        }
    }
    
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if ((ModeratorsSelection == ModeratorsSelection.AllMods && !Config.Moderators.Contains(context.User.Id)) 
            || (ModeratorsSelection == ModeratorsSelection.OnlyJamie && Config.Jamie.Id != context.User.Id))
        {
            RemoveFromStats(context);
            await context.Interaction.RespondAsync(ErrorString, ephemeral: true);
            return PreconditionResult.FromError(string.Empty);
        }

        return PreconditionResult.FromSuccess();
    }
}

public enum ModeratorsSelection
{
    AllMods,
    OnlyJamie
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