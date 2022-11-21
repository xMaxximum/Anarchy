using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord
{
    public static class SubscriptionExtensions
    {
        public static async Task<DiscordActiveSubscription> GetActiveSubscriptionAsync(this RestClient<IUserClient> client)
        {
            var activeSubscriptions = (await client.HttpClient.GetAsync("/users/@me/billing/subscriptions"))
                                                        .Deserialize<IReadOnlyList<DiscordActiveSubscription>>();

            return activeSubscriptions.Count > 0 ? activeSubscriptions[0] : null;
        }

        public static DiscordActiveSubscription GetActiveSubscription(this RestClient<IUserClient> client)
        {
            return client.GetActiveSubscriptionAsync().GetAwaiter().GetResult();
        }

        public static async Task<DiscordActiveSubscription> PurchaseSubscriptionAsync(this RestClient<IUserClient> client, ulong paymentMethodId, ulong skuId, uint additionalBoosts = 0)
        {
            List<AdditionalSubscriptionPlan> plans = new List<AdditionalSubscriptionPlan>();

            if (additionalBoosts > 0)
                plans.Add(new AdditionalSubscriptionPlan() { Id = DiscordNitroSubTypes.GuildBoost.SubscriptionPlanId, Quantity = (int) additionalBoosts });

            return (await client.HttpClient.PostAsync("/users/@me/billing/subscriptions", $"{{\"plan_id\":{skuId},\"payment_source_id\":{paymentMethodId},\"additional_plans\":{JsonConvert.SerializeObject(plans)}}}"))
                                .Deserialize<DiscordActiveSubscription>();
        }

        public static DiscordActiveSubscription PurchaseSubscription(this RestClient<IUserClient> client, ulong paymentMethodId, ulong skuId, uint additionalBoosts = 0)
        {
            return client.PurchaseSubscriptionAsync(paymentMethodId, skuId, additionalBoosts).GetAwaiter().GetResult();
        }
    }
}
