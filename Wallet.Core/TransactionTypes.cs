namespace TGS.Services.WalletService;

public enum TransactionTypes
{
    Deposit = 0,
    Withdrawal = 1,
    CompanyAdjust = 2,
    Bonus = 3,
    Rebate = 4,
    Dividend = 5,
    Promotion = 6,
    GameProviderFundOut = 7,
    GameProviderFundIn = 8,
    CancelWithdrawal = 9,
    GameProviderDeposit = 10,
    GameProviderWithdrawal = 11,
    UnsettledBetManual = 12,

    Other = 99,

    /// <summary>
    /// 存款 1000~1999
    /// </summary>
    //Deposit = 1000,
    /// <summary>
    /// 提款 2000~2999
    /// </summary>
    //Withdraw = 2000,
    /// <summary>
    /// 活動 3000~3999
    /// </summary>
    //Promotion = 3000,

    /// <summary>
    /// The promotion vip level upgrade
    /// </summary>
    PromotionVipLevelUpgrade = 3800,

    /// <summary>
    /// The promotion vip level season
    /// </summary>
    PromotionVipLevelSeason = 3850,

    /// <summary>
    /// 分紅 4000~4999
    /// </summary>
    //Dividend = 4000,
    /// <summary>
    /// The dividend all agent rebate
    /// </summary>
    DividendAllAgentRebate = 4100,

    /// <summary>
    /// 調帳 5000~5999
    /// </summary>
    //Adjust = 5000,
    AgentAdjust = 5100,

    SystemAdjust = 5200,

    /// <summary>
    /// 已結算單再調整
    /// </summary>
    AdjustResettledBetManual = 5010,

    /// <summary>
    /// 遊戲 10000~10999
    /// </summary>
    Game = 10000,

    PlaceBet = 10100,
    WinBet = 10200,
#pragma warning disable CA1069 // 列舉值不應重複
    LoseBet = 10200,
#pragma warning restore CA1069 // 列舉值不應重複
    FreeBet = 10300,
    TieBet = 10400,
    CancelTransaction = 10500,
    LockAmount = 10600,
    UnlockAmount = 10700,
    WinJackpot = 10800
}
