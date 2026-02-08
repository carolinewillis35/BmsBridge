using System.Text.Json.Nodes;

public sealed class E3GetDefaultLogGroupOperation : E3BaseDeviceOperation
{
    public override string Name => "GetDefaultLogGroup";

    public E3GetDefaultLogGroupOperation(Uri endpoint, ILoggerFactory loggerFactory)
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
