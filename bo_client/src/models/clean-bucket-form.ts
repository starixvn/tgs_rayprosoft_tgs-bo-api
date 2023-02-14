interface CleanBucketForm {
    id: string;
    playerCode: string;
    depositFormId: string;
    reviewBalance: number;
    reviewBucketAmount: number;
    cleaningBalance?: number;
    cleaningBucketAmount?: number;
    createdOn: Date;
    status: string;
}
