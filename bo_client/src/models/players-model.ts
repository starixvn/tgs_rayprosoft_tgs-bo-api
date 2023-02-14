interface PlayersModel {
    playerCode: string;
    parentUsername: string;
    status: string;
    username: string;
    agentName?: string;
    identityFormId?: string;
    realName?: string;
    registeredTime?: Date;
    registerHost?: string;
    vipLevel?: number;
}