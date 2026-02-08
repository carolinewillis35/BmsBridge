using System.Text.Json.Nodes;

public sealed class E3GetAlarmsOperation : E3BaseDeviceOperation
{
    public override string Name => "GetAlarms";

    public E3GetAlarmsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"]?["alarms"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
