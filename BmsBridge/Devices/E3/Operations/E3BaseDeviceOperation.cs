using System.Text.Json.Nodes;
using System.Text.Json;

public abstract class E3BaseDeviceOperation : BaseDeviceOperation
{
    protected virtual JsonObject? Parameters => null;

    protected E3BaseDeviceOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory) { }

    protected override IReadOnlyDictionary<string, string> DefaultHeaders =>
        new Dictionary<string, string>
        {
            ["Connection"] = "close",
            ["Content-Type"] = "application/json"
        };

    protected override HttpRequestMessage BuildRequest()
    {
        var payload = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = Name,
            ["id"] = "0",
        };

        if (Parameters is not null)
            payload.Add("params", Parameters);

        var formDict = new Dictionary<string, string>
        {
            ["m"] = JsonSerializer.Serialize(payload)
        };

        var formUrlEncoded = new FormUrlEncodedContent(formDict);

        var query = formUrlEncoded.ReadAsStringAsync().Result;
        var newUrl = $"{Endpoint}?{query}";

        var request = new HttpRequestMessage(HttpMethod.Post, newUrl)
        {
            Content = new StringContent("")
        };

        return request;
    }

    protected override JsonNode? Translate(HttpResponseMessage response)
        => JsonNode.Parse(response.Content.ReadAsStringAsync().Result);
}
