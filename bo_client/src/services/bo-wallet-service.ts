import { AxiosInstance } from "axios";

export default class BoWalletService {
    constructor(private http: AxiosInstance) { }

    public getPlayerMainWalletBalance(playerCode: string): Promise<number> {
        return this.http
            .get(`/api/wallet/player/${playerCode} `)
            .then(res => res.data);
    }

    public getPlayerAccountChangesOld(
        request: GetPlayerAccountChangesReportRequest): Promise<PagedResponse<PlayerAccountChangeReportModel>> {
        return this.http
            .get(
                `/api/player/accountChanges`,
                {
                    params: {
                        ...request
                    }
                })
            .then(res => res.data);
    }

    public getPlayerAccountChanges(
        playerCode: string,
        request: AccountChangesQueryRequest,
        page: number,
        rows: number
    ): Promise<PagedResponse<AccountChange>> {
        return this.http
            .get(
                `/api/wallet/account-changes/player/${playerCode}`,
                {
                    params: {
                        ...request,
                        page,
                        rows
                    }
                }
            )
            .then(res => res.data);
    }

    public getAgentAccountChangesOld(
        request: GetAgentAccountChangesReportRequest
    ): Promise<PagedResponse<AgentAccountChangeReportModel>> {
        return this.http
            .get(
                `/api/agent/accountChanges`,
                {
                    params: {
                        ...request
                    }
                })
            .then(res => res.data);
    }

    public getAgentAccountChanges(
        agentId: string,
        request: AccountChangesQueryRequest,
        page: number,
        rows: number
    ): Promise<PagedResponse<AccountChange>> {
        return this.http
            .get(
                `/api/wallet/account-changes/agent/${agentId}`,
                {
                    params: {
                        ...request,
                        page,
                        rows
                    }
                }
            )
            .then(res => res.data);
    }

    public getPlayerAccountChangesTotalAmountOld(request: GetPlayerAccountChangesReportRequest)
        : Promise<PlayerAccountChangesTotalModel> {
        return this.http
            .get(
                `/api/player/accountChanges/totalAmount`,
                {
                    params: {
                        ...request
                    }
                })
            .then(res => res.data);
    }

    public getPlayerAccountChangesTotalAmount(
        playerCode: string,
        request: AccountChangesQueryRequest
    ): Promise<number> {
        return this.http
            .get(
                `/api/wallet/account-changes/player/totalAmount/${playerCode}`,
                {
                    params: {
                        ...request
                    }
                }
            )
            .then(res => res.data);
    }

    public getPlayerWaitPersistentAccountChanges(
        playerCode: string,
        page: number,
        rows: number
    ): Promise<PagedResponse<AccountChange>> {
        return this.http
            .get(
                `/api/wallet/account-changes/player/waiting/${playerCode}`,
                {
                    params: {
                        page,
                        rows
                    }
                }
            )
            .then(res => res.data);
    }
}