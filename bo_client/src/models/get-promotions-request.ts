interface GetPromotionsRequest {
    brandId: string;
    from?: Date;
    to?: Date;
    name?: string;
    promotionType?: string;
    status?: string;
}