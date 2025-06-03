using System.Text.Json.Serialization;

public class RouteDecision
{
    [JsonPropertyName("RouteType")]
    public RouteType RouteType { get; set; }
    
    [JsonPropertyName("Confidence")]
    public double Confidence { get; set; }
    
    [JsonPropertyName("Reasoning")]
    public string Reasoning { get; set; } = string.Empty;
}