using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace TGS.Apis.BackendOffice.ViewModels;

public class BrandGameCreditTxnAddViewModel
{
    [Required]
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Money { get; init; }
    [Required]
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Amount { get; init; }
    public string Remark { get; init; }
    public JsonNode Expansions { get; init; }
}
