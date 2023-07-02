using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TT.Deliveries.Data.Dto;

public class OrderDto
{
    [Required]
    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; }

    [Required]
    [JsonPropertyName("sender")]
    public string Sender { get; set; }
}