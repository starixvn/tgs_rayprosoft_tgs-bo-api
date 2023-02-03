interface SubmitManualDepositEWalletRequest {
    playerCode: string;
    brandId: string;
    amount: number;
    brandPaymentId: string;
    promotionId?: string;
    bankType: string;
    note?: string;
    realName?: string;
    phone?: string;
}