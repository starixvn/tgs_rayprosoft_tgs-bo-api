interface PlayerBankInfo {
    id: string;
    paymentChannelId: string;
    bankType: string;
    detail: {};
    isDefault: boolean;
    updatedOn: Date;
    modifier?: string;
}