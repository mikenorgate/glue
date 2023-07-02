using MongoDB.Bson.Serialization.Attributes;

namespace TT.Deliveries.Data.Mongo;

public class Delivery
{
    [BsonId] public string OrderId { get; set; }

    public string Sender { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime? ApprovalTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public DateTime? CancellationTime { get; set; }
    public Recipient Recipient { get; set; }
}