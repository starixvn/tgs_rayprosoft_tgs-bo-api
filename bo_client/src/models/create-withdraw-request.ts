interface CreateWithdrawRequest {
    brandId: string;
    playerCode: string;
    amount: number;
    bankInfoId: string;
    note?: string;
}