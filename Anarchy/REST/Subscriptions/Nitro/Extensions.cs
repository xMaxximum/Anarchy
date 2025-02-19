using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace Discord
{
    public static class NitroSubscriptionExtensions
    {
        public static async Task<IReadOnlyList<DiscordGuildSubscription>> BoostGuildAsync(this RestClient<IUserAccount> client, ulong guildId, IEnumerable<ulong> boostSlots)
        {
            return (await client.HttpClient.PutAsync($"/guilds/{guildId}/premium/subscriptions", $"{{\"user_premium_guild_subscription_slot_ids\":{JsonSerializer.Serialize(boostSlots)}}}"))
                                .Deserialize<IReadOnlyList<DiscordGuildSubscription>>().SetClientsInList(client);
        }

        public static IReadOnlyList<DiscordGuildSubscription> BoostGuild(this RestClient<IUserAccount> client, ulong guildId, IEnumerable<ulong> boosts)
        {
            return client.BoostGuildAsync(guildId, boosts).GetAwaiter().GetResult();
        }

        public static async Task RemoveGuildBoostAsync(this RestClient<IUserAccount> client, ulong guildId, ulong subscriptionId)
        {
            await client.HttpClient.DeleteAsync($"/guilds/{guildId}/premium/subscriptions/{subscriptionId}");
        }

        public static void RemoveGuildBoost(this RestClient<IUserAccount> client, ulong guildId, ulong subscriptionId)
        {
            client.RemoveGuildBoostAsync(guildId, subscriptionId).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<DiscordBoostSlot>> GetBoostSlotsAsync(this RestClient<IUserAccount> client)
        {
            return (await client.HttpClient.GetAsync("/users/@me/guilds/premium/subscription-slots")).Deserialize<List<DiscordBoostSlot>>().SetClientsInList(client);
        }

        public static IReadOnlyList<DiscordBoostSlot> GetBoostSlots(this RestClient<IUserAccount> client)
        {
            return client.GetBoostSlotsAsync().GetAwaiter().GetResult();
        }

        public static async Task<DiscordActiveSubscription> SetAdditionalBoostsAsync(this RestClient<IUserAccount> client, ulong paymentMethodId, ulong activeSubscriptionId, uint amount)
        {
            string plan = JsonSerializer.Serialize(new AdditionalSubscriptionPlan() { Id = DiscordNitroSubTypes.GuildBoost.SubscriptionPlanId, Quantity = (int) amount });

            return (await client.HttpClient.PatchAsync("/users/@me/billing/subscriptions/" + activeSubscriptionId, $"{{\"payment_source_id\":{paymentMethodId},\"additional_plans\":[{plan}]}}")).Deserialize<DiscordActiveSubscription>();
        }

        public static DiscordActiveSubscription SetAdditionalBoosts(this RestClient<IUserAccount> client, ulong paymentMethodId, ulong activeSubscriptionId, uint amount)
        {
            return client.SetAdditionalBoostsAsync(paymentMethodId, activeSubscriptionId, amount).GetAwaiter().GetResult();
        }
    }
}

