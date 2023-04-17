interface GetPromotionsRequest extends PageableRequest {
    brandId: string;
    from?: Date;
    to?: Date;
    name?: string;
    promotionType?: string;
    status?: string;
}