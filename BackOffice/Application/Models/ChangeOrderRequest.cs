using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models;

public class ChangeOrderRequest : IRequest<MessageResponse>
{
    public ChangeOrderModel[] Orders { get; set; }
}

public class ChangeOrderModel
{
    public int DisplayOrder { get; set; }

    public Guid Id { get; set; }
}
