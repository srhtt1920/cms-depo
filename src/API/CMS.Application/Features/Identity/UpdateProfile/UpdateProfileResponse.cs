namespace CMS.Application.Features.Identity.UpdateProfile;

public sealed record UpdateProfileResponse(
Guid Id,
string Email,
string? DisplayName,
DateTime? LastLoginAt);
