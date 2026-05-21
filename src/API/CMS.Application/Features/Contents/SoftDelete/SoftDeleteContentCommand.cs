using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.SoftDelete;

public sealed record SoftDeleteContentCommand(Guid ContentId) : IRequest<Result>;
