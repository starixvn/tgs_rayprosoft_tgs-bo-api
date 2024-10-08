using System.IO;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Filters
{
    public class MerchanPermissionBodyFiler : BaseMerchanPermissionFilter
    {
        private readonly string propertyName;

        public MerchanPermissionBodyFiler(
            ILogger<BaseMerchanPermissionFilter> logger,
            IMerchantRepository merchantRepository,
            string checkName, string propertyName) : base(logger, merchantRepository, checkName)
        {
            this.propertyName = propertyName;
        }

        protected override string GetValue(ActionExecutingContext context)
        {
            var requestBody = context.ActionArguments["request"];
            return requestBody.GetType().GetProperty(propertyName).GetValue(requestBody).ToString();
        }
    }
}