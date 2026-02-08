using System.Text.Json.Nodes;

public sealed class E3GetAppsForLogGroupOperation : E3BaseDeviceOperation
{
    protected override JsonObject? Parameters => _parameters;
    private readonly JsonObject _parameters;

    public override string Name => "GetAppsForLogGroup";

    public E3GetAppsForLogGroupOperation(Uri endpoint, string lgiid, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        _parameters = new JsonObject
        {
            ["lgiid"] = lgiid
        };
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"] as JsonObject;

        if (response is null)
            return new JsonArray();

        return new JsonArray { response.DeepClone() };
    }
}
