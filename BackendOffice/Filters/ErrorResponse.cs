using System.Collections.Generic;

namespace TGS.Apis.BackendOffice.Filters;

public class ErrorResponse
{
    public List<ErrorModel> Errors => new();
}
