using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("/")]
public class HomeController : ControllerBase
{
    [Route("/")]
    [HttpGet]
    [Description("Home")]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return Ok("BackOffice API");
    }
}