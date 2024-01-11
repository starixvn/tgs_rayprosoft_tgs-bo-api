interface WithdrawReviewRequest {
    brandId: string;
    brandPaymentId?: string;
    remark?: string;
    reviewResponse: ReviewStatus;
}