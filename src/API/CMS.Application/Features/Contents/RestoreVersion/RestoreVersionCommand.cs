using CMS.SharedKernel.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Application.Features.Contents.RestoreVersion;

public sealed record RestoreVersionCommand(Guid ContentId, int VersionNumber) 
    : IRequest<Result>;
