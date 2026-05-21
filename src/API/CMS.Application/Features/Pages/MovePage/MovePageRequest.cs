namespace CMS.Application.Features.Pages.MovePage;

public sealed record MovePageRequest(Guid? NewParentId, int NewOrder);
