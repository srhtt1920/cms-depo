using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.UpdateProfile;

public sealed class UpdateProfileHandler(
 IUserRepository userRepository,
 ICurrentUser currentUser)
 : IRequestHandler<UpdateProfileCommand, Result<UpdateProfileResponse>>
{
    public async Task<Result<UpdateProfileResponse>> Handle(
        UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(
            UserId.From(currentUser.UserId), ct);

        if (user is null)
            return Result.Failure<UpdateProfileResponse>(
                Error.NotFound("User.NotFound", "Kullanıcı bulunamadı."));

        // Email başka kullanıcıda var mı?
        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await userRepository.GetByEmailAsync(request.Email, ct);
            if (existing is not null && existing.Id != user.Id)
                return Result.Failure<UpdateProfileResponse>(
                    Error.Conflict("User.EmailTaken", $"'{request.Email}' zaten kullanımda."));
        }

        user.UpdateProfile(request.DisplayName, request.Email);
        await userRepository.UpdateAsync(user, ct);

        return new UpdateProfileResponse(
            user.Id.Value, user.Email, user.DisplayName, user.LastLoginAt);
    }
}
