using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TT.Deliveries.Data.Dto;

public class AccessWindowDto
{
    [Required]
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [Required]
    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }
}