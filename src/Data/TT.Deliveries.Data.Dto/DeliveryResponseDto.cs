using System.Text.Json.Serialization;

namespace TT.Deliveries.Data.Dto;

public class DeliveryResponseDto
{
    [JsonPropertyName("state")] public DeliveryState State { get; set; }

    [JsonPropertyName("accessWindow")] public AccessWindowDto AccessWindow { get; set; }

    [JsonPropertyName("recipient")] public RecipientDto Recipient { get; set; }

    [JsonPropertyName("order")] public OrderDto Order { get; set; }
}