using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.GetSuperAdmins;

public sealed record GetSuperAdminsQuery : IRequest<Result<List<SuperAdminListDto>>>;

[RequirePermission("superadmin")]
public sealed class GetSuperAdminsHandler(IUserRepository userRepo)
    : IRequestHandler<GetSuperAdminsQuery, Result<List<SuperAdminListDto>>>
{
    public async Task<Result<List<SuperAdminListDto>>> Handle(
        GetSuperAdminsQuery _, CancellationToken ct)
    {
        // SuperAdmin claim'i olan kullanıcıları getir.
        // Basit implementasyon: tüm kullanıcılar arasında IsSuperAdmin bayrağı olanlara bak.
        // Alternatif: claim tabanlı — "role=SuperAdmin" claim'i olan JWT'ler.
        var allUsers = await userRepo.GetSuperAdminsAsync(ct);
        return allUsers.Select(u => new SuperAdminListDto(
            u.Id.Value, u.Email, u.DisplayName, u.CreatedAt)).ToList();
    }
}
