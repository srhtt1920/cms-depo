using CMS.SharedKernel.DynamicQuery;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.ListRoles;

public sealed record ListRolesQuery(
    int PageIndex = 0,
    int PageSize = 10,
    DynamicQuery? DynamicQuery = null) : IRequest<Result<ListRolesResponse>>;
