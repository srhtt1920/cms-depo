using CMS.Application.Common.Serialization;
using System.Text.Json;

namespace CMS.Infrastructure.Serialization;

public sealed class JsonSerializerAdapter(JsonSerializerOptions options) : ISerializer
{
    public string ContentType => "application/json";

    public byte[] Serialize<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, options);

    public T? Deserialize<T>(byte[] data) =>
        JsonSerializer.Deserialize<T>(data, options);
}
