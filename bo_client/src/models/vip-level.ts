interface Viplevel {
    birthBenefit: number;
    cardGameRebate: number;
    id: string;
    level: number;
    levelKeepAccumulateBetAmount: number;
    levelKeepAccumulateDeposit: number;
    levelKeepCondition: boolean;
    levelUpAccumulateBetAmount: number;
    levelUpAccumulateDeposit: number;
    liveDealerRebate: number;
    lotteryRebate: number;
    promotionBenefit: number;
    quarterlyBenefit: number;
    slotRebate: number;
    sportsRebate: number;
    virtualSportsRebate: number;
    withdrawTotalAmountPerDay: number;
    withdrawTransactionPerDay: number;
    brandId: string;
    createdOn: Date;
    createdByUsername?: string;
    updatedOn: Date;
    updatedByUsername?: string;
}