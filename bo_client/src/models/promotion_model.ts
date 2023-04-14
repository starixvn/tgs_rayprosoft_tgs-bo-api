interface PromotionModel {
    id: string;
    name: string;
    status: string;
    from: Date;
    to: Date;
    promotionType: string;
    number: number;
    totalAmountLimit?: number;
    unlimited?: boolean;
    amountUsed: number;
    vipLevels: number[];
}