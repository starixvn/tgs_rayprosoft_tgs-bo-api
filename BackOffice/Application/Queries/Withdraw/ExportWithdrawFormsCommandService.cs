using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ClosedXML.Report;
using Microsoft.Extensions.FileProviders;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Withdraw;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

internal class ExportWithdrawFormsCommandService : IGetService<ExportWithdrawFormsCommand, MemoryStream>
{
    private static readonly EmbeddedFileProvider _EmbeddedFileProvider =
        new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IFileInfo _templateFileInfo;

    public ExportWithdrawFormsCommandService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient =
            paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _templateFileInfo = _EmbeddedFileProvider.GetFileInfo("/Assets/withdraw_forms_template.xlsx");
    }

    public async ValueTask<MemoryStream> GetAsync(ExportWithdrawFormsCommand query, CancellationToken cancellationToken)
    {
        var resultPaymentMethods = await _paymentServiceClient.GetAllPaymentMethodsAsync()
            .ToDictionaryAsync(x => x.Id).ConfigureAwait(false);

        var resource = new ResourceManager(typeof(Resources.Withdraw));

        var exportData = query.WithdrawForms.Select((x, index) => new ExportWithdrawModel
        {
            SerialNumber = index + 1,
            Username = x.UserName,
            UserType = resource.GetString(x.UserType.ToString()) ?? x.UserType.ToString(),
            Id = x.Id.ToString(),
            OrderNo = x.OrderNo,
            ThirdPartyOrderNo = x.ThirdPartyOrderNo,
            Amount = x.ActualAmount ?? x.Amount,
            ApplicationTime = x.CreatedOn.ToOffset(TimeSpan.FromHours(8D)).DateTime,
            UpdateTime = x.UpdatedOn.ToOffset(TimeSpan.FromHours(8D)).DateTime,
            Status = resource.GetString(x.Status.ToString()) ?? x.Status.ToString(),
            Remark = x.Remark
        });

        using var workbook = new XLWorkbook();

        GenerateWorkSheet(workbook, exportData, query.TotalAmount);

        var ms = new MemoryStream();

        workbook.SaveAs(ms);
        ms.Position = 0;

        return ms;
    }

    private void GenerateWorkSheet(
        XLWorkbook target,
        IEnumerable<ExportWithdrawModel> data,
        decimal totalAmount)
    {
        using var fs = _templateFileInfo.CreateReadStream();
        using var template = new XLTemplate(fs);

        template.AddVariable(new
        {
            Rows = data,
            TotalAmount = totalAmount
        });

        template.Generate();

        template.Workbook.Worksheet("template").CopyTo(target, "Withdraw");
    }
}
