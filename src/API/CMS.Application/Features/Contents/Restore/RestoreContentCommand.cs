using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Restore;
public sealed record RestoreContentCommand(Guid ContentId) : IRequest<Result>;
