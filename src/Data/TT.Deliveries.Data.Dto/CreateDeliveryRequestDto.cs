using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TT.Deliveries.Data.Dto;

public class CreateDeliveryRequestDto
{
    [Required]
    [JsonPropertyName("accessWindow")]
    public AccessWindowDto AccessWindow { get; set; }

    [Required]
    [JsonPropertyName("recipient")]
    public RecipientDto Recipient { get; set; }

    [Required] [JsonPropertyName("order")] public OrderDto Order { get; set; }
}