interface GetAgentAccountChangesReportRequest extends PageableRequest {
    agentId: string;
    brandId: string;
    from: Date;
    to: Date;
    transactionType?: string;
}