using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RouteType
{
    CustomerService,
    TechnicalSupport,
    GeneralInquiry,
    Escalation
}