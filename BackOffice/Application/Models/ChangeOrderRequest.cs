using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models;

public class ChangeOrderRequest : IRequest<MessageResponse>
{
    public ChangeOrderModel[] Orders { get; set; }
}

public class ChangeOrderModel
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
}