using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users;

public class GetUsersRequest
{
    public string Username { get; set; }

    public string DepartmentName { get; set; }

    public string Role { get; set; }
}
