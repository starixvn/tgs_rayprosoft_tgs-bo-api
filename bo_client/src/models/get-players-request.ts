interface GetPlayersRequest extends PageableRequest{
    brandId: string;
    agentName?: string;
    bankInfoTag?: string;
    bankInfoTagType?: string;
    beginRegisteredOn?: string;
    endRegisteredOn?: string;
    parentUsername?: string;
    phone?: string;
    playerCode?: string;
    realName?: string;
    registerHost?: string;
    status?: string;
    username?: string;
    vipId?: string;    
}