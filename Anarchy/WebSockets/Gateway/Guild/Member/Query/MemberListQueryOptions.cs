using System.Text.Json.Serialization;
namespace Discord.Gateway
{
    public class MemberListQueryOptions
    {
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}

