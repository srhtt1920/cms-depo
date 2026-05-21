using CMS.Application.Common.Serialization;

namespace CMS.Infrastructure.Serialization;

public sealed class SerializerFactory(IEnumerable<ISerializer> serializers) : ISerializerFactory
{
    private readonly Dictionary<string, ISerializer> _byContentType =
        serializers.ToDictionary(s => s.ContentType, StringComparer.OrdinalIgnoreCase);

    public ISerializer Create(SerializerType type) => type switch
    {
        SerializerType.Json        => _byContentType["application/json"],
        SerializerType.MessagePack => _byContentType["application/x-msgpack"],
        _ => throw new NotSupportedException($"Serializer type '{type}' is not supported.")
    };

    public ISerializer CreateFromContentType(string contentType)
    {
        if (_byContentType.TryGetValue(contentType, out var serializer))
            return serializer;

        // Fallback: JSON
        return _byContentType["application/json"];
    }
}
