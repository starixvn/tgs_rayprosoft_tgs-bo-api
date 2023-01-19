interface ReducePlayerAmountRequest {
    includeInNetProfit: boolean;
    transactionType: string;
    amount: number;
    remark?: string;
    bucketAmount: number;
}