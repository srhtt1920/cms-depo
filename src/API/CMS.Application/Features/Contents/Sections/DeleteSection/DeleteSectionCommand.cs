using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.DeleteSection;
public sealed record DeleteSectionCommand(Guid ContentId, Guid SectionId) : IRequest<Result>;
