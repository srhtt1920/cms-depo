using CMS.Application.Common.Serialization;
using MessagePack;

namespace CMS.Infrastructure.Serialization;

public sealed class MessagePackSerializerAdapter : ISerializer
{
    public string ContentType => "application/x-msgpack";

    public byte[] Serialize<T>(T value) =>
        MessagePackSerializer.Serialize(value);

    public T? Deserialize<T>(byte[] data) =>
        MessagePackSerializer.Deserialize<T>(data);
}
