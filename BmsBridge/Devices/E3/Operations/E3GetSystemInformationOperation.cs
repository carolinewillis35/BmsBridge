using System.Text.Json.Nodes;

public sealed class E3GetSystemInformationOperation : E3BaseDeviceOperation
{
    public override string Name => "GetSystemInformation";

    public E3GetSystemInformationOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"] as JsonObject;

        if (response is null)
            return new JsonArray();

        return new JsonArray { response.DeepClone() };
    }
}
