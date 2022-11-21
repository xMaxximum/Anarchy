using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord
{
    public static class BillingExtensions
    {
        public static async Task<IReadOnlyList<DiscordPayment>> GetPaymentsAsync(this RestClient<IUserClient> client, int limit = 100)
        {
            return (await client.HttpClient.GetAsync("/users/@me/billing/payments?limit=" + limit))
                                .Deserialize<IReadOnlyList<DiscordPayment>>();
        }

        public static IReadOnlyList<DiscordPayment> GetPayments(this RestClient<IUserClient> client, int limit = 100)
        {
            return client.GetPaymentsAsync(limit).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(this RestClient<IUserClient> client)
        {
            return (await client.HttpClient.GetAsync("/users/@me/billing/payment-sources"))
                                      .MultipleDeterministic<PaymentMethod>().SetClientsInList(client);
        }

        public static IReadOnlyList<PaymentMethod> GetPaymentMethods(this RestClient<IUserClient> client)
        {
            return client.GetPaymentMethodsAsync().GetAwaiter().GetResult();
        }
    }
}
