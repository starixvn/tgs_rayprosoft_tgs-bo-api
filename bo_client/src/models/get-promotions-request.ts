interface GetPromotionsRequest extends PageableRequest {
    brandId: string;
    from?: Date;
    to?: Date;
    name?: string;
    promotionTypes?: string[];
    status?: string;
}