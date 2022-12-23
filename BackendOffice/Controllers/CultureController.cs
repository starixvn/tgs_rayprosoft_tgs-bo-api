using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CultureController : BaseContextController
{
    private readonly IStringLocalizer _localizer;

    public CultureController(
        IStringLocalizer<CultureController> localizer)
    {
        _localizer = localizer;
    }

    [Route("")]
    [HttpPost]
    [Description("Change culture")]
    public MessageResponse ChangeCulture(string culture)
    {
        SetCulture(culture);

        return new MessageResponse
        {
            Message = _localizer.GetString("test")
        };
    }

    private void SetCulture(string culture)
    {
        var lang = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
        Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, lang);

        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
    }
}
