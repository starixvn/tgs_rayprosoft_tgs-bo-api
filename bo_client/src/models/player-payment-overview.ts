interface PlayerPaymentOverview {
    firstDepositAmount?: number;
    firstDepositHandlingFee?: number;
    firstDepositOn?: Date;
    firstWithdrawAmount?: number;
    firstWithdrawHandlingFee?: number;
    firstWithdrawOn?: Date;
    recordedBy: Date;
    totalDepositAmount: number;
    totalWithdrawAmount: number;
    totalDepositCount: number;
    totalWithdrawCount: number;
}