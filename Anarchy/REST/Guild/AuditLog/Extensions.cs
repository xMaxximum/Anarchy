using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;


namespace Discord
{
    public static class AuditLogExtensions
    {
        public static async Task<IReadOnlyList<AuditLogEntry>> GetAuditLogAsync(this IRestClient client, ulong guildId, AuditLogFilters filters = null)
        {
            filters ??= new AuditLogFilters();

            return (await client.HttpClient.GetAsync($"/guilds/{guildId}/audit-logs?{(filters.UserIdProperty.Set ? $"user_id={filters.UserId}" : "")}&{(filters.ActionTypeProperty.Set ? $"action_type={(int) filters.ActionType}" : "")}&{(filters.BeforeIdProperty.Set ? $"before={filters.BeforeId}" : "")}&{(filters.LimitProperty.Set ? $"limit={filters.Limit}" : "")}"))
                                .Body["audit_log_entries"].Deserialize<List<AuditLogEntry>>();
        }

        /// <summary>
        /// Gets the audit log for the specified guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>A <see cref="IReadOnlyList{AuditLogEntry}"/></returns>
        public static IReadOnlyList<AuditLogEntry> GetAuditLog(this IRestClient client, ulong guildId, AuditLogFilters filters = null)
        {
            return client.GetAuditLogAsync(guildId, filters).GetAwaiter().GetResult();
        }
    }
}
