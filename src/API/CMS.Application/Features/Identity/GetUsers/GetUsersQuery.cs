using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetUsers;

public sealed record GetUsersQuery : IRequest<Result<List<UserListDto>>>;

public sealed record UserListDto(
    Guid Id,
    string Email,
    string? DisplayName,
    List<string> Roles,
    bool IsActive,
    DateTime? LastLogin,
    int PermissionVersion);
