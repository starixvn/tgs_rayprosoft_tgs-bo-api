interface GetPlayerResponse extends MessageResponse {
    player: PlayerModel;    
}

interface PlayerModel {
    agentUsername?: string;
    areaCode?: string;
    balance: number;
    banks: BankModel[];
    birthdate?: string;
    email?: string;
    facebookMessenger?: string;
    idNumber?: string;
    lastLoginIp: string;
    lastLoginOn?: Date;
    line?: string;
    parentUsername?: string;
    phone?: string;
    playerCode: string;
    qQ?: string;
    realName?: string;
    registeredOn?: Date;
    registerHost: string;
    registerIp: string;
    remark?: string;
    status: string;
    statusName: string;
    telegram?: string;    
    username: string;
    viber?: string;
    vipId?: string;
    vipLevel?: number;
    wageringRequirement: number;
    weChat?: string;
    whatsApp?: string;
    identityAuthentication: boolean;    
}

interface BankModel {
    bankType: string;
    detail: string;
    id: string;
    isDefault: boolean;
    paymentChannelId: string;
}