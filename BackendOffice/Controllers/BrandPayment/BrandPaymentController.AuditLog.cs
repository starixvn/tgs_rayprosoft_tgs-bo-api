using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.BrandPayment;

	public partial class BrandPaymentController
	{
		[HttpGet("{paymentId:guid}/auditLogs")]
		[Description("Get a payment's audit log")]
		public async Task<GetBrandPaymentAuditLogResponse> GetAuditLogAsync(Guid paymentId)
		{
			var auditLogs = await _auditLogQueries.GetQueries().Where(q => q.BrandPaymentId == paymentId).ToArrayAsync();

			return new GetBrandPaymentAuditLogResponse
			{
				Valid = true,
				AuditLogs = (from log in auditLogs
							 let before = JsonSerializer.Deserialize<BrandPaymentAuditData>(log.Before)
							 let after = JsonSerializer.Deserialize<BrandPaymentAuditData>(log.After)
							 select new PaymentAuditLogModel
							 {
								 Id = log.Id,
								 PaymentId = log.BrandPaymentId,
								 BeforeChannelName = before.ChannelName,
								 AfterChannelName = after.ChannelName,
								 BeforePaymentStatus = before.PaymentStatus,
								 AfterPaymentStatus = after.PaymentStatus,
								 BeforeTransferLimitFrom = before.TransferLimitFrom,
								 AfterTransferLimitFrom = after.TransferLimitFrom,
								 BeforeTransferLimitTo = before.TransferLimitTo,
								 AfterTransferLimitTo = after.TransferLimitTo,
								 BeforePlayerListCondition = before.PlayerListCondition,
								 AfterPlayerListCondition = after.PlayerListCondition,
								 BeforeAllPlayers = before.AllPlayers,
								 AfterAllPlayers = after.AllPlayers,
								 BeforeIdentityAuthentication = before.IdentityAuthentication,
								 AfterIdentityAuthentication = after.IdentityAuthentication,
								 BeforePhoneAuthentication = before.PhoneAuthentication,
								 AfterPhoneAuthentication = after.PhoneAuthentication,
								 BeforeDepositedAmountUnlimited = before.DepositedAmountUnlimited,
								 AfterDepositedAmountUnlimited = after.DepositedAmountUnlimited,
								 BeforeDepositedAmountMin = before.DepositedAmountMin,
								 AfterDepositedAmountMin = after.DepositedAmountMin,
								 BeforeDepositedAmountMax = before.DepositedAmountMax,
								 AfterDepositedAmountMax = after.DepositedAmountMax,
								 Reason = log.Reason,
								 CreatedOn = log.CreatedOn,
								 CreatedByUsername = log.CreatedByUsername
							 }).ToArray()
			};
		}
	}
