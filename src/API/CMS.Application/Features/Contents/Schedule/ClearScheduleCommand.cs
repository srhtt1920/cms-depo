using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Schedule;
public sealed record ClearScheduleCommand(Guid ContentId) : IRequest<Result>;
