namespace CMS.Application.Common.Serialization;

public interface ISerializer
{
    string ContentType { get; }
    byte[] Serialize<T>(T value);
    T? Deserialize<T>(byte[] data);
}
