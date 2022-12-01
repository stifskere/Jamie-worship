using Discord;
using Discord.Interactions;
using Discord.WebSocket;
namespace JamieWorshipper.Handlers;

public class PagedEmbedHandler<TListType>
{

    public int CurrentIndex { get; set; }
    public IUser Author { get; set; } = null!;
    public uint EmbedColor { get; set; }

    public struct ListConfiguration
    {
        public string Prefix { get; init; }
        public string ErrorEmptyTitle { get; init; }
        public string ErrorEmptyDescription { get; init; }
        public string CommandName { get; init; }
        public string ListEmbedTitle { get; init; }
    }
    
    // ReSharper disable StaticMemberInGenericType
    private static readonly ButtonBuilder BackButton = new ButtonBuilder()
        .WithStyle(ButtonStyle.Secondary)
        .WithLabel("Back")
        .WithEmote(new Emoji("⬅"));
        
    private static readonly ButtonBuilder ForwardButton = new ButtonBuilder()
        .WithStyle(ButtonStyle.Secondary)
        .WithLabel("Forward")
        .WithEmote(new Emoji("➡"));
    // ReSharper enable StaticMemberInGenericType

    private static readonly Dictionary<string, (List<TListType> list, ListConfiguration configuration, FieldDelegate predicate)> CurrentLists = new();
    private static readonly Dictionary<ulong, PagedEmbedHandler<TListType>> CurrentInstances = new();

    public delegate Task<EmbedFieldBuilder> FieldDelegate(TListType listIteration);

    public PagedEmbedHandler(ListConfiguration config, List<TListType> list,
        SocketInteractionContext context, FieldDelegate predicate)
        => ConstructorAsync(config, list, context, predicate);

    private static EmbedBuilder GetPageEmbed(int start, uint embedColor, List<TListType> list, ListConfiguration config, FieldDelegate predicate)
        => new EmbedBuilder()
            .WithTitle(config.ListEmbedTitle)
            .WithFields(list.Skip(start).Take(5).Select(item => predicate(item)).Select(r => r.Result))
            .WithColor(embedColor)
            .WithFooter($"Showing {start + 1}-{start + 6} out of {list.Count}");

    private static bool _listButtonsEventAdded;
    public static async Task ListButtonsEvent(SocketMessageComponent component)
    {
        if (!new[] {"-Back", "-Forward"}.Any(c => component.Data.CustomId.Contains(c))) return;

        await component.DeferAsync(ephemeral: true);
        
        string prefix = component.Data.CustomId.Split('-')[0];
        ListConfiguration currentConfig = CurrentLists[prefix].configuration;
        List<TListType> currentList = CurrentLists[prefix].list;
        FieldDelegate currentPredicate = CurrentLists[prefix].predicate;

        if (!CurrentInstances.ContainsKey(component.Message.Id))
        {
            await component.FollowupAsync($"This interaction expired. Lists expire after 15 minutes, run `{currentConfig.CommandName}` again.");
            return;
        }

        PagedEmbedHandler<TListType> thisInstance = CurrentInstances[component.Message.Id];

        if (component.User.Id != thisInstance.Author.Id)
        {
            await component.FollowupAsync($"This interaction is not yours, run `{currentConfig.CommandName}` yourself.");
            return;
        }

        _ = component.Data.CustomId.Contains("-Back") ? thisInstance.CurrentIndex -= 5 : thisInstance.CurrentIndex += 5;

        await component.Message.ModifyAsync(m =>
        {
            ComponentBuilder instanceComponentBuilder = new ComponentBuilder();
            if (thisInstance.CurrentIndex != 0)
                instanceComponentBuilder =
                    instanceComponentBuilder.WithButton(BackButton.WithCustomId($"{prefix}-Back"));
            if (thisInstance.CurrentIndex + 5 < currentList.Count)
                instanceComponentBuilder =
                    instanceComponentBuilder.WithButton(ForwardButton.WithCustomId($"{prefix}-Forward"));

            m.Embed = GetPageEmbed(thisInstance.CurrentIndex, thisInstance.EmbedColor, currentList, currentConfig,
                currentPredicate).Build();
            m.Components = instanceComponentBuilder.Build();
        });
        
        CurrentInstances[component.Message.Id] = thisInstance;
    }
    
    private async void ConstructorAsync(ListConfiguration config, List<TListType> list, SocketInteractionContext context, FieldDelegate predicate)
    {
        await context.Interaction.DeferAsync();
        
        EmbedColor = RandomColor();
        CurrentIndex = 0;
        Author = context.User;

        if (list.Count < 1)
        {
            await context.Interaction.ModifyOriginalResponseAsync(r => r.Embed = new Optional<Embed>(new EmbedBuilder()
                .WithTitle(config.ErrorEmptyTitle)
                .WithDescription(config.ErrorEmptyDescription)
                .WithColor(EmbedColor)
                .Build()));
            return;
        }

        await context.Interaction.ModifyOriginalResponseAsync(r =>
        {
            r.Embed = new Optional<Embed>(GetPageEmbed(0, EmbedColor, list, config, predicate).Build());
            r.Components = list.Count <= 5 ? null : new ComponentBuilder().WithButton(ForwardButton.WithCustomId($"{config.Prefix}-Forward")).Build();
        });
        
        if (list.Count <= 5) return;

        ulong interactionMessageId = (await context.Interaction.GetOriginalResponseAsync()).Id;
        
        CurrentInstances.Add(interactionMessageId, this);
        CurrentLists[config.Prefix] = (list, config, predicate);

        async void RemoveInstanceThreadHandler(ulong id)
        {
            await Task.Delay(TimeSpan.FromMinutes(15));
            CurrentInstances.Remove(id);
        }

        new Thread(() => RemoveInstanceThreadHandler(interactionMessageId)).Start();

        if (_listButtonsEventAdded) return;
        Client.ButtonExecuted += ListButtonsEvent;
        _listButtonsEventAdded = true;
    }
}