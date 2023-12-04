using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TGS.Apis.BackendOffice.ViewModels;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/status")]
[ApiController]
public class StatusController(IConfiguration configuration) : BaseContextController
{
    [Route("")]
    [HttpGet]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Description("Get status")]
    public Task<StatusViewModel> GetStatusAsync()
    {
        var version = configuration.GetValue<string>("Version");

        return Task.FromResult(new StatusViewModel 
        { 
            Version = version 
        });
    }
}
