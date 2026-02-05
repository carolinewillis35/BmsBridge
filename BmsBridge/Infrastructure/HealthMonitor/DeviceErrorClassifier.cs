using System.Text.Json;
using System.Net.Sockets;

public static class DeviceErrorClassifier
{
    public static DeviceErrorType Classify(Exception ex)
    {
        return ex switch
        {
            TaskCanceledException => DeviceErrorType.Timeout,
            TimeoutException => DeviceErrorType.Timeout,
            HttpRequestException httpEx when httpEx.InnerException is SocketException =>
                DeviceErrorType.ConnectionFailed,
            HttpRequestException => DeviceErrorType.HttpError,
            JsonException => DeviceErrorType.JsonParseError,
            _ => DeviceErrorType.InternalException
        };
    }

    public static DeviceErrorType Classify(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => DeviceErrorType.Unauthorized,
                System.Net.HttpStatusCode.BadGateway => DeviceErrorType.HostUnreachable,
                System.Net.HttpStatusCode.ServiceUnavailable => DeviceErrorType.HostUnreachable,
                _ => DeviceErrorType.HttpError
            };
        }

        return DeviceErrorType.None;
    }
}
