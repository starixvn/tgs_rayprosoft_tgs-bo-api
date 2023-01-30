interface PaymentOverview {
    firstDepositAmount?: number;
    firstDepositHandlingFee?: number;
    firstDepositOn?: Date;
    firstWithdrawAmount?: number;
    firstWithdrawHandlingFee?: number;
    firstWithdrawOn?: Date;
    recordedBy: string;
    totalDepositAmount: number;
    totalWithdrawAmount: number;
}