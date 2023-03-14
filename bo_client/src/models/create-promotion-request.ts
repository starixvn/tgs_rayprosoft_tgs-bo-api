interface CreatePromotionRequest {
    autoSentPlayerBonus: boolean;
    bonusAmountLimit: number;
    brandId: string;
    content?: string;
    desktopBanner?: string;
    from: string;
    gameProviderIds?: string[];
    gameType?: string;
    localization?: string;
    mobileBanner?: string;
    name: string;
    percentage: number;
    promotionType: string;
    rule?:string;    
    status: number;
    subSubject: string;
    terms?: string;
    to: string;
    totalAmountLimit?: number;
    turnoverMultiple: number;
    unlimited: boolean;
    vipLevels: number[];
}