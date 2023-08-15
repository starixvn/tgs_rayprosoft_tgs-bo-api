interface PlayerAccountChangeReportModel{
    transactionId: string;
    gameactionId: string;
    amount: number;
    balance: number;
    transactionType: string[];
    gameactionType: string[];
    recordedOn: Date;
    includeInNetProfit: boolean;
    remark: string;
    externalTransactionId: string;
}