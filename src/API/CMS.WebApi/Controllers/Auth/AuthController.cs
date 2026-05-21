using CMS.Application.Features.Identity.ChangePassword;
using CMS.Application.Features.Identity.ForgotPassword;
using CMS.Application.Features.Identity.Login;
using CMS.Application.Features.Identity.Logout;
using CMS.Application.Features.Identity.RefreshToken;
using CMS.Application.Features.Identity.ResetPassword;
using CMS.Application.Features.Identity.SelectTenant;
using CMS.Application.Features.Identity.TwoFactor.Disable;
using CMS.Application.Features.Identity.TwoFactor.Resend;
using CMS.Application.Features.Identity.TwoFactor.Setup;
using CMS.Application.Features.Identity.TwoFactor.Verify;
using CMS.Application.Features.Identity.UpdateProfile;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CMS.WebApi.Controllers.Auth;
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ApiController
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    /// <summary>
    /// Dashboard içi tenant switch — SelectTenant ile ayný logic.
    /// JWT yenilenir; client tüm state'i (cache, navigation) resetler.
    /// </summary>
    [HttpPost("switch-tenant")]
    [Authorize]
    public async Task<IActionResult> SwitchTenant(
        [FromBody] SelectTenantCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct) =>
        HandleResult(await sender.Send(new UpdateProfileCommand(null, ""), ct));

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpPut("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordCommand command, CancellationToken ct) =>
    HandleResult(await sender.Send(command, ct));

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<IActionResult> TwoFactorSetup(CancellationToken ct) =>
      HandleResult(await sender.Send(new TwoFactorSetupCommand(), ct));

    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> TwoFactorVerify(
        [FromBody] TwoFactorVerifyCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> TwoFactorDisable(
        [FromBody] TwoFactorDisableCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    [HttpPost("2fa/resend")]
    [AllowAnonymous]
    public async Task<IActionResult> TwoFactorResend(
        [FromBody] TwoFactorResendCommand command, CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));
}