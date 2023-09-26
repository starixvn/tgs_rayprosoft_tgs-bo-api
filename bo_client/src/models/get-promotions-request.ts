interface GetPromotionsRequest extends PageableRequest {
    brandId: string;
    from?: string;
    to?: string;
    name?: string;
    promotionTypes?: string[];
    status?: string;
}