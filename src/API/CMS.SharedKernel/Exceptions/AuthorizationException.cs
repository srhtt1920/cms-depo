namespace CMS.SharedKernel.Exceptions;

public sealed class AuthorizationException(string message) : Exception(message);
