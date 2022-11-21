using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Discord
{
    // Anarchy's gift support might be a little shakey right now
    // Things may have been changed (or i've ignored them for whatever reason), so i'll patch everything up once i have data
    public static class GiftsExtensions
    {

        public static async Task<string> PurchaseGiftAsync(this RestClient<IUserClient> client, ulong paymentMethodId, ulong skuId, ulong subPlanId, int expectedAmount)
        {
            return (await client.HttpClient.PostAsync($"/store/skus/{skuId}/purchase", new PurchaseOptions()
            {
                PaymentMethodId = paymentMethodId,
                SkuPlanId = subPlanId,
                ExpectedAmount = expectedAmount
            })).Deserialize<JObject>().Value<string>("gift_code");
        }

        public static string PurchaseGift(this RestClient<IUserClient> client, ulong paymentMethodId, ulong skuId, ulong subPlanId, int expectedAmount)
        {
            return client.PurchaseGiftAsync(paymentMethodId, skuId, subPlanId, expectedAmount).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<DiscordGift>> GetGiftInventoryAsync(this RestClient<IUserClient> client)
        {
            return (await client.HttpClient.GetAsync("/users/@me/entitlements/gifts")).Deserialize<IReadOnlyList<DiscordGift>>();
        }

        public static IReadOnlyList<DiscordGift> GetGiftInventory(this RestClient<IUserClient> client)
        {
            return client.GetGiftInventoryAsync().GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<RedeemableDiscordGift>> QueryGiftCodesAsync(this RestClient<IUserClient> client, ulong skuId, ulong subPlanId)
        {
            return (await client.HttpClient.GetAsync($"/users/@me/entitlements/gift-codes?sku_id={skuId}&subscription_plan_id={subPlanId}"))
                                .Deserialize<IReadOnlyList<RedeemableDiscordGift>>().SetClientsInList(client);
        }

        public static IReadOnlyList<RedeemableDiscordGift> QueryGiftCodes(this RestClient<IUserClient> client, ulong skuId, ulong subPlanId)
        {
            return client.QueryGiftCodesAsync(skuId, subPlanId).GetAwaiter().GetResult();
        }

        public static async Task<RedeemableDiscordGift> CreateGiftCodeAsync(this RestClient<IUserClient> client, ulong skuId, ulong subPlanId)
        {
            return (await client.HttpClient.PostAsync("/users/@me/entitlements/gift-codes", $"{{\"sku_id\":{skuId},\"subscription_plan_id\":{subPlanId}}}"))
                                .Deserialize<RedeemableDiscordGift>().SetClient(client);
        }

        public static RedeemableDiscordGift CreateGiftCode(this RestClient<IUserClient> client, ulong skuId, ulong subPlanId)
        {
            return client.CreateGiftCodeAsync(skuId, subPlanId).GetAwaiter().GetResult();
        }

        public static async Task RevokeGiftCodeAsync(this RestClient<IUserClient> client, string code)
        {
            await client.HttpClient.DeleteAsync("/@me/entitlements/gift-codes/" + code);
        }

        public static void RevokeGiftCode(this RestClient<IUserClient> client, string code)
        {
            client.RevokeGiftCodeAsync(code).GetAwaiter().GetResult();
        }

        public static async Task RedeemGiftAsync(this RestClient<IUserClient> client, string code, ulong? channelId = null)
        {
            await client.HttpClient.PostAsync($"/entitlements/gift-codes/{code}/redeem", channelId.HasValue ? $"{{\"channel_id\":{channelId.Value}}}" : null);
        }

        public static void RedeemGift(this RestClient<IUserClient> client, string code, ulong? channelId = null)
        {
            client.RedeemGiftAsync(code, channelId).GetAwaiter().GetResult();
        }

        public static async Task<RedeemableDiscordGift> GetGiftAsync(this RestClient<IUserClient> client, string code)
        {
            return (await client.HttpClient.GetAsync($"/entitlements/gift-codes/{code}?with_application=false&with_subscription_plan=true"))
                                .Deserialize<RedeemableDiscordGift>();
        }

        public static RedeemableDiscordGift GetGift(this RestClient<IUserClient> client, string code)
        {
            return client.GetGiftAsync(code).GetAwaiter().GetResult();
        }

        public static async Task<string> PurchaseNitroGiftAsync(this RestClient<IUserClient> client, ulong paymentMethodId, DiscordNitroSubType type)
        {
            return await client.PurchaseGiftAsync(paymentMethodId, type.SkuId, type.SubscriptionPlanId, type.ExpectedAmount);
        }

        public static string PurchaseNitroGift(this RestClient<IUserClient> client, ulong paymentMethodId, DiscordNitroSubType type)
        {
            return client.PurchaseGift(paymentMethodId, type.SkuId, type.SubscriptionPlanId, type.ExpectedAmount);
        }
    }
}
