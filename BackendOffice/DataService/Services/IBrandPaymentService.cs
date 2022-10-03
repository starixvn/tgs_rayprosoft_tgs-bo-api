using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.DataService.Services;

public interface IBrandPaymentService
{
    Task ChangeOrderAsync(ChangeOrderRequest request);

    Task ChangeStatusAsync(BrandPayment payment, SwitchStatus status, string username);

    Task CreatePaymentAsync(CreateBrandPaymentRequest request, PaymentTransferType transferType, string username);

    Task<bool> ExistsAsync(Expression<Func<BrandPayment, bool>> predicate);

    Task<BrandPayment> GetPaymentAsync(Expression<Func<BrandPayment, bool>> predicate,
                                        Func<IQueryable<BrandPayment>, IQueryable<BrandPayment>> preQuery = null, bool tracking = false);

    IQueryable<BrandPayment> GetPaymentQueries(bool tracking = false);

    Task UpdatePaymentAsync(BrandPayment payment, UpdateBrandPaymentRequest request, string username);
}
