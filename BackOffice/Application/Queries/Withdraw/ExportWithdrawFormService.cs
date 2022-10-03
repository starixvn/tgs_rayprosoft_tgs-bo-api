using System;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Utility;
using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Models.Withdraw;
using ClosedXML.Excel;
using ClosedXML.Report;
using System.Collections.Generic;
using TGS.Services.BackOffice.Application.Models.Deposit;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

internal class ExportWithdrawFormService : IGetService<ExportWithdrawForm, MemoryStream>
{
    private static readonly EmbeddedFileProvider _EmbeddedFileProvider =
        new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IFileInfo _templateFileInfo;

    public ExportWithdrawFormService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient =
            paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _templateFileInfo = _EmbeddedFileProvider.GetFileInfo("/Assets/withdraw_forms_template.xlsx");
    }

    public async ValueTask<MemoryStream> GetAsync(ExportWithdrawForm query, CancellationToken cancellationToken)
    {
        var result = await _paymentServiceClient.QueryWithdrawFormsAsync(
            new WithdrawFormQueryConditions
            {
                BrandId = query.BrandId,
                ChannelName = query.ChannelName,
                CreateOnRange = query.CreateOnRange,
                PaymentMethodId = query.PaymentMethodId,
                Status = query.Status,
                ThirdPartyOrderNo = query.ThirdPartyOrderNo,
                FormId = query.FormId,
                UpdateOnRange = query.UpdateOnRange,
                UserName = query.UserName
            },
            new Tgs.Payment.Clients.Paging
            {
                IsDesc = query.Paging.IsDesc,
                Page = query.Paging.Page,
                Rows = query.Paging.Rows,
                ShowAll = true,
                SortBy = query.Paging.SortBy
            }).ConfigureAwait(false);

        var resultPaymentMethods = await _paymentServiceClient.GetAllPaymentMethodsAsync()
            .ToDictionaryAsync(x => x.Id).ConfigureAwait(false);

        var resource = new ResourceManager(typeof(Resources.Withdraw));

        var exportData = result.Items?.Select((x, index) => new ExportWithdrawModel
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

        GenerateWorkSheet(workbook, exportData, result.Sum);

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