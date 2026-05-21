using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.SetUserActive;

[RequirePermission("users.manage")]
public sealed class SetUserActiveHandler(IUserRepository userRepository)
    : IRequestHandler<SetUserActiveCommand, Result>
{
    public async Task<Result> Handle(SetUserActiveCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(UserId.From(request.UserId), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        if (request.IsActive) user.Activate();
        else                  user.Deactivate();

        await userRepository.UpdateAsync(user, ct);
        return Result.Success();
    }
}
