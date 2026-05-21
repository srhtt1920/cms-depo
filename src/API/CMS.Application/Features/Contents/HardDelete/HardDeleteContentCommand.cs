using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.HardDelete;
public sealed record HardDeleteContentCommand(Guid ContentId) : IRequest<Result>;
