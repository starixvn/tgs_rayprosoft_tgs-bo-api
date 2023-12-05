interface PlayerAccountChange {
    transactionId: string;
    amount: number;
    lockAmount: number;
    balance: number;
    lockedBalance: number;
    transactionType: string;
    recordedOn: Date;
    includeInNetProfit: boolean;
    remark?: string;
    externalTransactionId?: string;
}