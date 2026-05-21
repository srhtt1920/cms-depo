using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.UpdateLanguages;

public sealed record UpdateLanguagesCommand(
    Guid TenantId,
    List<string> LanguageCodes,
    string DefaultLanguageCode) : IRequest<Result>;
