interface SubmitManualDepositBankRequest {
    playerCode: string;
    brandId: string;
    bankAccount: string;
    bankName?: string;
    amount: number;
    brandPaymentId: string;
    promotionId?: string;
    bankType: string;
    note?: string;
    realName?: string;
}