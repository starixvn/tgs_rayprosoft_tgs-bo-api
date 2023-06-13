interface AdjustRecordQueryRequest {
    from: Date;
    to: Date;
    type?: string[];
    userId?: string;
    operator?: string;
}