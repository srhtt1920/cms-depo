using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.ChangePassword;

public sealed record ChangePasswordCommand(
string CurrentPassword,
string NewPassword,
string ConfirmPassword) : IRequest<Result>;
