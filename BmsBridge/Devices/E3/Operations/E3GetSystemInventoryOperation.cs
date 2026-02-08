using System.Text.Json.Nodes;

public sealed class E3GetSystemInventoryOperation : E3BaseDeviceOperation
{
    public override string Name => "GetSystemInventory";

    public E3GetSystemInventoryOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"]?["aps"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
