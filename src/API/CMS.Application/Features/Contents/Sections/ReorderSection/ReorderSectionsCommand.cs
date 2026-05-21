using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.ReorderSection;

public sealed record ReorderSectionsCommand(
    Guid ContentId,
    List<SectionOrderItem> Items) : IRequest<Result>;

public sealed record SectionOrderItem(Guid SectionId, int Order);
