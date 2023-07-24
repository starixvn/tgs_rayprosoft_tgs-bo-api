using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TGS.Apis.BackendOffice.ViewModels;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/users")]
[ApiController]
public class StatusController : BaseContextController
{
    private readonly IConfiguration _configuration;

    public StatusController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Route("")]
    [HttpGet]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Description("Get status")]
    public Task<StatusViewModel> GetStatusAsync()
    {
        var version = _configuration.GetValue<string>("Version");

        return Task.FromResult(new StatusViewModel 
        { 
            Version = version 
        });
    }
}
