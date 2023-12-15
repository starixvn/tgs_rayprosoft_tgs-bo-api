using System;
using System.Text.Json.Nodes;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

public record BrandGameCreditTxnAddDebitCommand(
    Guid BrandId,
    decimal Money,
    decimal Amount,
    string Remark,
    JsonNode Expansions,
    Guid CreatedUserId,
    string CreatedUsername);
