interface createOfflineLoginRewardRequest {
    name: string,
    from: Date,
    to: Date,
    bonusAmount: number,
    turnoverMultiple: number,
    batchId: string;
}