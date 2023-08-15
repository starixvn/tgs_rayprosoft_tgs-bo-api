interface GetPlayerAccountChangesReportRequest extends PageableRequest{
    accountChangeType?: string[];
    brandId: string;
    from: Date;
    playerCode: string;
    to: Date;
}