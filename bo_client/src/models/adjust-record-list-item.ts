interface AdjustRecordListItem {
    id: string;
    brandId: string;
    brandCode: string;
    adjustType: string;
    userId: string;
    userType: string;
    amount: number;
    bucketAmount: number;
    remark?: string;
    operator: string;
    createdOn: Date;
}