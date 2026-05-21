using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.UpdateProfile;

public sealed record UpdateProfileCommand(string? DisplayName, string Email)
  : IRequest<Result<UpdateProfileResponse>>;
