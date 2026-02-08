using System.Text.Json.Nodes;

public sealed class DanfossReadMetersOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_meters";

    public DanfossReadMetersOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"];

        if (response is null)
            return new JsonArray();

        return new JsonArray { response.DeepClone() };
    }
}
