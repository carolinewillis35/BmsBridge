using System.Text.Json.Nodes;

public sealed class DanfossReadHvacsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_hvacs";

    public DanfossReadHvacsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"]?["hvacs"]?["hvac"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
