namespace CMS.Application.Features.SuperAdmin;

public sealed record TransferUserResponse(
    bool Success,
    string? Message,
    int ContentTransferred,
    List<string> Warnings);
