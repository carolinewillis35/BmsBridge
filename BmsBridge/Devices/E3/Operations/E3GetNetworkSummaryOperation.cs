using System.Text.Json.Nodes;

public sealed class E3GetNetworkSummaryOperation : E3BaseDeviceOperation
{
    public override string Name => "GetNetworkSummary";

    public E3GetNetworkSummaryOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"]?["devices"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
