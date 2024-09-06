namespace TGS.Services.BackOffice.Domain.Enums;

public enum TransactionType
{
    Deposit = 0,
    Withdrawal = 1,

    /// <summary>
    /// 公司調帳
    /// </summary>
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
    GameProviderDeductBalance = 13,
    GameProviderAddBalance = 14,
    GameProviderAdjustBalance = 15,

    Other = 99,

    /// <summary>
    /// 存款 1000~1999
    /// </summary>
    //Deposit = 1000,
    Deposited = 1100,

    /// <summary>
    /// 提款 2000~2999
    /// </summary>
    //Withdraw = 2000,
    WithdrawLock = 2100,

    WithdrawCommit = 2200,
    WithdrawRollback = 2300,
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

    PromotionVipLevelBirthday = 3860,

    /// <summary>
    /// 分紅 4000~4999
    /// </summary>
    //Dividend = 4000,
    /// <summary>
    /// The dividend all agent rebate
    /// </summary>
    DividendAllAgentRebate = 4100,

    AgentDividend = 4200,

    /// <summary>
    /// 調帳 5000~5999
    /// </summary>
    //Adjust = 5000,
    AgentAdjust = 5100,

    SystemAdjust = 5200,
    TransferAdjust = 5300,
    BonusAdjust = 5400,

    /// <summary>
    /// 已結算單再調整
    /// </summary>
    AdjustResettledBetManual = 5010,

    // 轉帳 6000~6999
    Transfer = 6100,

    TransferCommit = 6200,
    TransferRollback = 6300,
    Receive = 6400,
    ReceiveCommit = 6500,
    ReceiveRollback = 6600,

    /// <summary>
    /// The game
    /// 遊戲 10000~15999
    /// </summary>
    //Game = 10000

    //Credit上分、Debit下分、Refund退款、Revision修正
    GameDebit = 10100,

    GameCredit = 10200,
    GameRefund = 10300,
    GameRevision = 10400,

    // 下注11000~11999
    GameBet = 11000,

    // 結算 12000~12999

    GameSettle = 12000,
    GameSettleWin = 12100,
    GameSettleLose = 12200,
    GameSettleFree = 12300,
    GameSettleTie = 12400,
    GameWinJackpot = 12500,

    // 取消 13000~13999

    GameCancel = 13000,

    // 鎖定相關 14000~14999

    GameLock = 14100,
    GameUnlock = 14200,

    // 遊戲供應 15000~15999
    GameProvider = 15000,

    GameProviderFundOutLock = 15110,
    GameProviderFundOutCommit = 15120,
    GameProviderFundOutRollback = 15130,

    // 遊戲供應資金退款
    GameProviderFundRefund = 15300,
}