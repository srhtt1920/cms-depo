using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.CreateRole;

/// <summary>
/// TenantId: ITenantContext'ten otomatik alýnýr — Blazor'dan gönderilmesine gerek yok.
/// PermissionKeys: string key listesi ("content.read" gibi) — GUID yerine key kullanýlýr.
/// </summary>
public sealed record CreateRoleCommand(
    string Name,
    List<string> PermissionKeys) : IRequest<Result<CreateRoleResponse>>;
