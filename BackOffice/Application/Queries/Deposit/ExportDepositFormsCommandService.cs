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
using TGS.Services.BackOffice.Application.Models.Deposit;

namespace TGS.Services.BackOffice.Application.Queries.Deposit;

public class ExportDepositFormsCommandService : IGetService<ExportDepositFormsCommand, MemoryStream>
{
    private static readonly EmbeddedFileProvider _EmbeddedFileProvider =
        new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IFileInfo _templateFileInfo;

    public ExportDepositFormsCommandService(
        IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _templateFileInfo = _EmbeddedFileProvider.GetFileInfo("/Assets/deposit_forms_template.xlsx");
    }

    public async ValueTask<MemoryStream> GetAsync(ExportDepositFormsCommand query, CancellationToken cancellationToken)
    {
        var resultPaymentMethods = await _paymentServiceClient.GetAllPaymentMethodsAsync()
            .ToDictionaryAsync(x => x.Id)
            .ConfigureAwait(false);

        var resource = new ResourceManager(typeof(Resources.Deposit));

        var exportData = query.DepositForms.Select((row, index) => new ExportDepositModel
        {
            SerialNumber = index + 1,
            UserName = row.UserName,
            Id = row.Id.ToString(),
            OrderNo = row.OrderNo,
            ThirdPartyOrderNo = row.ThirdPartyOrderNo,
            PaymentMethod = resource.GetString(resultPaymentMethods[row.PaymentMethodId].Name),
            PaymentName = row.ChannelName,
            Amount = row.Amount,
            ActualAmount = row.ActualAmount,
            ApplicationTime = row.CreatedOn.ToOffset(TimeSpan.FromHours(8D)).DateTime,
            UpdateTime = row.UpdatedOn.ToOffset(TimeSpan.FromHours(8D)).DateTime,
            Status = resource.GetString(row.Status.ToString()) ?? row.Status.ToString(),
            Remark = row.Remark,
            HandlingFee = row.Status == PaymentFlowState.Approved ? row.HandlingFee : null,
            ReviewedUserName = row.ReviewedUserName
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
        IEnumerable<ExportDepositModel> data,
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

        template.Workbook.Worksheet("template").CopyTo(target, "Deposit");
    }
}
