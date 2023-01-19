interface AdjustPlayerAmountRequest {
    includeInNetProfit: boolean;
    transactionType: string;
    amount: number;
    remark?: string;
    bucketAmount: number;
}