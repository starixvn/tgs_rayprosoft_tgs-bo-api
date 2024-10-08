using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Filters
{
    public class MerchanPermissionQueryFiler : BaseMerchanPermissionFilter
    {
        private readonly string routerName;

        public MerchanPermissionQueryFiler(
            ILogger<BaseMerchanPermissionFilter> logger,
            IMerchantRepository merchantRepository,
            string checkName, string routerName) : base(logger, merchantRepository, checkName)
        {
            this.routerName = routerName;
        }

        protected override string GetValue(ActionExecutingContext context)
        {
            return context.HttpContext.Request.Query[routerName];
        }
    }
}