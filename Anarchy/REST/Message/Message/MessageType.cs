using System.Text.Json.Serialization;
namespace Discord
{
    public enum MessageType
    {
        Default,
        RecipientAdd,
        RecipientRemove,
        Call,
        ChannelNameChange,
        ChannelIconChange,
        ChannelPinnedMessage,
        GuildMemberJoin,
        GuildBoosted,
        GuildBoostedTier1,
        GuildBoostedTier2,
        GuildBoostedTier3,
        ChannelFollowAdd,
        GuildDiscoveryDisgualified = 14,
        GuildDiscoveryRequalified,
        GuildDiscoveryGracePeriodInitialWarning,
        GuildDiscoveryGracePeriodFinalWarning,
        ThreadCreated,
        Reply,
        ChatInputCommand,
        ThreadStarterMessage,
        GuildInviteReminder,
        ContextMenuCommand
    }
}

