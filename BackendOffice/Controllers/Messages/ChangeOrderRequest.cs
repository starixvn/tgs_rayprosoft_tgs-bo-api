using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages;

public class ChangeOrderRequest
{
    public ChangeOrderModel[] Orders { get; set; }
}

public class ChangeOrderModel
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
}