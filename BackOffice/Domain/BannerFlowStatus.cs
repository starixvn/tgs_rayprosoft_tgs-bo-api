namespace TGS.Services.BackOffice.Domain;

public enum BannerFlowStatus
{
    WaitApproving = 0,
    Approved = 1000,
    Rejected = 2000,
    Unknown = 9000
}
