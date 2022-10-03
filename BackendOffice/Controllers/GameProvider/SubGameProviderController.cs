using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Controllers.Messages.SubGameProvider;
using TGS.Apis.BackendOffice.Services;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public class SubGameProviderController : BaseContextController
{
    private readonly IMediator _mediator;
    private readonly IUploadService _uploadService;

    public SubGameProviderController(IMediator mediator, IUploadService uploadService)
    {
        _mediator = mediator;
        _uploadService = uploadService;
    }

    [HttpGet]
    [Description("Get sub game providers")]
    public Task<PagedResponse<SubGameProviderModel>>
        GetSubGameProvidersAsync([FromQuery] GetSubGameProvidersRequest request) => _mediator.Send(request);

    [HttpGet("{code}")]
    [Description("Get a sub game provider")]
    public Task<GetSubGameProviderResponse> GetSubGameProviderAsync(string code) =>
        _mediator.Send(new GetSubGameProviderRequest(code));

    [HttpPost]
    [Description("Create a sub game provider")]
    public Task<MessageResponse> CreateGameProviderAsync(CreateSubGameProviderRequest request) =>
        _mediator.Send(request);

    [HttpPut("{id}/{code}")]
    [Description("Update a sub game provider")]
    public Task<MessageResponse> UpdateGameProviderAsync(Guid id, string code, UpdateSubGameProviderRequest request)
    {
        request.GameProviderId = id;
        request.Code = code;
        return _mediator.Send(request);
    }
}