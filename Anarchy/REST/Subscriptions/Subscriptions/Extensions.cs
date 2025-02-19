using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace Discord
{
    public static class SubscriptionExtensions
    {
        public static async Task<DiscordActiveSubscription> GetActiveSubscriptionAsync(this RestClient<IUserAccount> client)
        {
            var activeSubscriptions = (await client.HttpClient.GetAsync("/users/@me/billing/subscriptions"))
                                                        .Deserialize<IReadOnlyList<DiscordActiveSubscription>>();

            return activeSubscriptions.Count > 0 ? activeSubscriptions[0] : null;
        }

        public static DiscordActiveSubscription GetActiveSubscription(this RestClient<IUserAccount> client)
        {
            return client.GetActiveSubscriptionAsync().GetAwaiter().GetResult();
        }

        public static async Task<DiscordActiveSubscription> PurchaseSubscriptionAsync(this RestClient<IUserAccount> client, ulong paymentMethodId, ulong skuId, uint additionalBoosts = 0)
        {
            List<AdditionalSubscriptionPlan> plans = new List<AdditionalSubscriptionPlan>();

            if (additionalBoosts > 0)
                plans.Add(new AdditionalSubscriptionPlan() { Id = DiscordNitroSubTypes.GuildBoost.SubscriptionPlanId, Quantity = (int) additionalBoosts });

            return (await client.HttpClient.PostAsync("/users/@me/billing/subscriptions", $"{{\"plan_id\":{skuId},\"payment_source_id\":{paymentMethodId},\"additional_plans\":{JsonSerializer.Serialize(plans)}}}"))
                                .Deserialize<DiscordActiveSubscription>();
        }

        public static DiscordActiveSubscription PurchaseSubscription(this RestClient<IUserAccount> client, ulong paymentMethodId, ulong skuId, uint additionalBoosts = 0)
        {
            return client.PurchaseSubscriptionAsync(paymentMethodId, skuId, additionalBoosts).GetAwaiter().GetResult();
        }
    }
}

