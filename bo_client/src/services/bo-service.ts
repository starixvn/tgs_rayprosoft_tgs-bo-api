import { AxiosInstance } from "axios";
import fs from 'fs';
import FormData from 'form-data';

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

    public getVipAdjustInfos(
        playerCode: string,
        brandId: string,
        from: string,
        to: string,
        page: number,
        rows: number,
        sortby: string,
        orderby: string,
        showAll: boolean): Promise<any> {
        return this.http
            .get(
                `/api/player/vipAdjustInfos/${playerCode}`,
                {
                    params: {
                        brandId: brandId,
                        from: from,
                        to: to,
                        page: page,
                        rows: rows,
                        sortby: sortby,
                        orderby: orderby,
                        showAll: showAll
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

    public submitDepositBank(request: SubmitManualDepositBankRequest): Promise<any> {
        return this.http
            .put('/api/manualdeposit/bank', request)
            .then(res => res.data);
    }

    public submitDepositEWallet(request: SubmitManualDepositEWalletRequest): Promise<any> {
        return this.http
            .put('/api/manualdeposit/ewallet', request)
            .then(res => res.data);
    }

    public submitDepositPulsa(request: SubmitManualDepositPulsaRequest): Promise<string> {
        return this.http
            .put('/api/manualdeposit/pulsa', request)
            .then(res => res.data);
    }

    public queryCleanBucketForms(playerCode: string, flowState?: string): Promise<CleanBucketForm[]> {
        return this.http
            .get(flowState === null || flowState === undefined
                ? `/api/player/bucket/clean-form/${playerCode}`
                : `/api/player/bucket/clean-form/${playerCode}/${flowState}`)
            .then(res => res.data);
    }

    public getPlayerList(        
        request: GetPlayersRequest 
        ): Promise<PagedResponse<PlayerModel>>{
        return this.http
            .get('/api/player',
                {
                    params: {
                        ...request
                    }
                }
            )
            .then(res => res.data);
    }

    public createOfflineLoginRewardPromotion(brandId: string, request: createOfflineLoginRewardRequest) : Promise<MessageResponse> {
        return this.http
        .post(`/api/offline-login-reward/create/${brandId}`, request)
            .then(res => res.data);
    }

    public checkOfflineLoginRewardBackgroundProcessCompleted(batchId: string) : Promise<void> {
        return this.http
        .head(`/api/offline-login-reward/${batchId}`)
        .then(res => res.data);
    }

    public exportOfflineLoginRewardFormTemplate() : Promise<any> {
        return this.http
        .get('/api/offline-login-reward/template')
        .then(res => res.data);
    }

    public uploadOfflineLoginRewardUserList(brandId: string) : Promise<string> {
        
        var stream = fs.createReadStream("assets/TestOfflineLoginReward.xlsx");

        var formData = new FormData();
        formData.append("file", stream, 'file.xlsx');

        var config = {
            headers: {
              'Content-Type': 'multipart/form-data'
            }
        };

        return this.http
            .post(`/api/offline-login-reward/${brandId}`, formData,  {
                headers: {
                    ...config.headers,
                    ...formData.getHeaders(),
                },
            })
            .then(res => res.data);

        // return this.http
        //     .post(`/api/offline-login-reward/${brandId}`, formData,  {
        //         headers: {
        //                 'Content-Type': 'multipart/form-data'
        //         }
        //     })
        //     .then(res => res.data);
        
        // return this.http
        //    .post(`/api/offline-login-reward/${brandId}`)
        //    .then(res => res.data);
    }
}