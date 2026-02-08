using System.Text.Json.Nodes;

public sealed class E3GetAppDescriptionOperation : E3BaseDeviceOperation
{
    public override string Name => "GetAppDescription";

    protected override JsonObject? Parameters => _parameters;
    private readonly JsonObject _parameters;

    public E3GetAppDescriptionOperation(Uri endpoint, string sessionId, string iid, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        _parameters = new JsonObject
        {
            ["iid"] = iid,
            ["sid"] = sessionId,
        };
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"]?["points"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
