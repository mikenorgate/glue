using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TT.Deliveries.Data.Dto;

public class RecipientDto
{
    [Required] [JsonPropertyName("name")] public string Name { get; set; }

    [Required]
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [Required]
    [Phone]
    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; }
}