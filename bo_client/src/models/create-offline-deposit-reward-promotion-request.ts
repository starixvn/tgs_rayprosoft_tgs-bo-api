interface CreateOfflineDepositRewardPromotionRequest {
    name: string,
    from: string,
    to: string,
    turnoverMultiple: number,
    batchId: string;
    phases: OfflineDepositPhase[];
}