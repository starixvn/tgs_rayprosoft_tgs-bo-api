interface WithdrawFormQuery {
    userName?: string;
    channel?: string;
    thirdPartyOrderNo?: string;
    id?: string;
    paymentId?: string;
    paymentMethod?: string;
    status?: string[];
    beginCreatedOn?: Date;
    endCreatedOn?: Date;
    beginUpdatedOn?: Date;
    endUpdatedOn?: Date;
    brandPaymentIds?: string[];
    userType?: string;
}