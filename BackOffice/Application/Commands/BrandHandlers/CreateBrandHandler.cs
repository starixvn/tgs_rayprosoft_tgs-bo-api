using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Tgs.BrandService.Protos;
using TGS.Core.Models.Messages;
using Tgs.MessageQueue;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandHandlers;

public class CreateBrandHandler : Handler<CreateBrandRequest, MessageResponse>
{
    private readonly IBrandService _service;
    private readonly IMessageSender _messageSender;

    public CreateBrandHandler(
        IBrandService service, 
        IHttpContextAccessor accessor,
        IMessageSender messageSender) 
        : base(accessor)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public override async Task<MessageResponse> Handle(CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        if (await _service.ExistsAsync(x => x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NameExisted);
        }

        if (await _service.ExistsAsync(x => x.Code.ToLower() == request.Code.ToLower()))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.CodeExisted);
        }

        var brandId = await _service.CreateBrandAsync(request, cancellationToken);

        await _messageSender.PublishAsync("tgs.bo.brand.created",
                new BrandCreatedEvent
                {
                    BrandId = brandId
                }.ToByteArray(),
                cancellationToken)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }
}
