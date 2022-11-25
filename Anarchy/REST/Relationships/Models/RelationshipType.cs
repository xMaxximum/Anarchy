using System.Text.Json.Serialization;
namespace Discord
{
    public enum RelationshipType
    {
        None,
        Friends,
        Blocked,
        IncomingRequest,
        OutgoingRequest
    }
}

