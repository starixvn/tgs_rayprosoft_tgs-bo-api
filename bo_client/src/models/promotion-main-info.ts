interface PromotionMainInfo {
    id: string,
    name: string;
    brandId: string,
    autoSentPlayerBonus: boolean;
    from: string;
    to: string;
    promotionType: string;
    totalAmountLimit?: number;
    unlimited: boolean;
}