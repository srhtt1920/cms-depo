using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.UpdateProfile;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetMe;

public sealed record GetMeQuery : IRequest<Result<UpdateProfileResponse>>;

/// <summary>
/// Kimliği doğrulanmış kullanıcının profil bilgilerini döner.
/// GET /api/auth/me endpoint'i tarafından kullanılır.
/// </summary>
public sealed class GetMeHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetMeQuery, Result<UpdateProfileResponse>>
{
    public async Task<Result<UpdateProfileResponse>> Handle(
        GetMeQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(
            UserId.From(currentUser.UserId), ct);

        if (user is null)
            return Result.Failure<UpdateProfileResponse>(
                Error.NotFound("User.NotFound", "Kullanıcı bulunamadı."));

        return new UpdateProfileResponse(
            user.Id.Value,
            user.Email,
            user.DisplayName,
            user.LastLoginAt);
    }
}
