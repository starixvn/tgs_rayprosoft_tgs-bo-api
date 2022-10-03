using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users;

public class GetUsersRequest : PageableRequest
{
    public string Username { get; set; }
}
