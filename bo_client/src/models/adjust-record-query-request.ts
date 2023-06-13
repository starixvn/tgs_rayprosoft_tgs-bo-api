interface AdjustRecordQueryRequest {
    from: Date;
    to: Date;
    type?: string[];
    user?: string;
    operator?: string;
}