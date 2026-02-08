using System.Text.Json.Nodes;

public sealed class E3GetAppTypesOperation : E3BaseDeviceOperation
{
    public override string Name => "GetAppTypes";

    public E3GetAppTypesOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"]?["apptypes"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
