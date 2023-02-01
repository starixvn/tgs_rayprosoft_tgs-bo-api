import { AxiosInstance } from "axios";

export default class BackOfficeService {
    constructor(private http: AxiosInstance) { }

    public adjustPlayerLockedAmount(playerCode: string, lockAmount: number, remark: string | null): Promise<void> {
        return this.http
            .put(`/api/player/wallet/locked/${playerCode}`, { lockAmount, remark })
            .then(res => res.data);
    }

    public getPlayerIpsByUserName(
        brandId: string,
        userName: string,
        page: number,
        rows: number,
        sortby: string,
        orderby: string,
        showAll: boolean): Promise<any> {
        return this.http
            .get(
                `/api/playerip/username`,
                {
                    params: {
                        brandId: brandId,
                        userName: userName,
                        page: page,
                        rows: rows,
                        sortby: sortby,
                        orderby: orderby,
                        showAll: showAll
                    }
                })
            .then(res => res.data);
    }

    public getPlayerPaymentOverview(playerCode: string): Promise<any> {
        return this.http
            .get(`/api/player/payment/${playerCode}`, {})
            .then(res => res.data);
    }

    public increasePlayerAmount(playerCode: string, request: AdjustPlayerAmountRequest): Promise<MessageResponse> {
        return this.http
            .post(`/api/player/wallet/increase/${playerCode}`, request)
            .then(res => res.data);
    }

    public reducePlayerAmount(playerCode: string, request: AdjustPlayerAmountRequest): Promise<MessageResponse> {
        return this.http
            .post(`/api/player/wallet/reduce/${playerCode}`, request)
            .then(res => res.data);
    }

    public listAgentPlayerWinloss(
        agentId: string,
        condition: AgentPlayerWinLossReportConditions,
        paging: PageableRequest): Promise<PagedResponse<AgentPlayerWinLoss>> {
        return this.http
            .get(
                `/api/agentreport/${agentId}/winloss`,
                {
                    params: {
                        ...condition,
                        ...paging
                    }
                })
            .then(res => res.data);
    }

    public addAgentDomain(brandId: string, agentId: string, domain: string): Promise<void> {
        return this.http
            .put(
                `/api/agent/domain/${brandId}/${agentId}`,
                null,
                {
                    params: {
                        domain
                    }
                })
            .then(res => res.data);
    }

    public listAgentDomains(brandId: string, agentId: string): Promise<ApiResponse<string[]>> {
        return this.http
            .get(`/api/agent/domain/${brandId}/${agentId}`)
            .then(res => res.data);
    }

    public removeAgentDomain(brandId: string, agentId: string, domain: string): Promise<void> {
        return this.http
            .delete(
                `/api/agent/domain/${brandId}/${agentId}`,
                {
                    params: {
                        domain
                    }
                })
            .then(res => res.data);
    }

    public clearPlayerBuckets(playerCode: string): Promise<void> {
        return this.http
            .delete(`/api/player/bucket/${playerCode}`)
            .then(res => res.data);
    }

    public adjustIncreasePlayerBucket(playerCode: string, request: AdjustPlayerBucketRequest): Promise<void> {
        return this.http
            .put(`/api/player/bucket/${playerCode}:adjust-increase`, request)
            .then(res => res.data);
    }
}