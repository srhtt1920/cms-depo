namespace CMS.Application.Common.Serialization;

public interface ISerializerFactory
{
    ISerializer Create(SerializerType type);
    ISerializer CreateFromContentType(string contentType);
}
