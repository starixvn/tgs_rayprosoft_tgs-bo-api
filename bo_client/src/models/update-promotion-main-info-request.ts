interface UpdatePromotionMainInfoRequest {
    name: string;
    autoSentPlayerBonus: boolean;
    from: string;
    to: string;
    promotionType: string;
    totalAmountLimit?: number;
    unlimited: boolean;
}