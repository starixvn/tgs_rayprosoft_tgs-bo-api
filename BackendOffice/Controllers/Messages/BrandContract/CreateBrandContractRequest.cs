using System;
using System.Collections.Generic;
using Tgs.Agent.Dividend.Client.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandContract;

public record CreateBrandContractRequest(
    Guid BrandId,
    string Name,
    short ContractDividendType,
    decimal GradeIntervalLimit,
    decimal ValidBetAmount,
    decimal DepositAmount,
    int BetDays,
    string EditorUsername,
    ContractGameType[] GameTypes,
    IEnumerable<CreateBrandContractConditionModel> Conditions);