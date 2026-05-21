using CMS.Application.Common.Serialization;

namespace CMS.WebApi.Middleware;

public sealed class SerializationMiddleware(RequestDelegate next)
{
    private const string MsgPackContentType = "application/x-msgpack";
    internal const string SerializerTypeKey = "SerializerType";

    public async Task InvokeAsync(HttpContext context)
    {
        var accept = context.Request.Headers.Accept.ToString();

        var serializerType = accept.Contains(MsgPackContentType)
            ? SerializerType.MessagePack
            : SerializerType.Json;

        context.Items[SerializerTypeKey] = serializerType;

        await next(context);
    }
}

