using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.BulkAction;

public enum BulkActionType { Publish, Archive, SoftDelete }

public sealed record BulkActionCommand(
    List<Guid> ContentIds,
    BulkActionType Action)
    : IRequest<Result<BulkActionResult>>;

public sealed record BulkActionResult(
    int Succeeded, int Failed, List<string> Errors);
