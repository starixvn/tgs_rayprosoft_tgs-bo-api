import { AxiosInstance } from "axios";
import fs from 'fs';
import FormData from 'form-data';
import path from 'path';

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
    ): Promise<PagedResponse<PlayerModel>> {
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

    public downloadBatchAdjustFormTemplate(): Promise<{ status: number; contentType: string }> {
        return this.http
            .get('/api/batch-adj-balance')
            .then(res => {
                let contentType: string;

                if (typeof res.headers.getContentType === 'string' || typeof res.headers.getContentType === 'undefined')
                    contentType = res.headers.getContentType ?? '';
                else
                    contentType = res.headers.getContentType()?.toString() ?? '';

                return {
                    status: res.status,
                    contentType: contentType
                };
            });
    }

    public async importBatchAdjustForm(
        brandId: string,
        clientToken: string | null,
        filePath: string): Promise<string> {

        let fileName = path.basename(filePath);
        let fileStream = fs.readFileSync(filePath);
        let formData = new FormData();

        formData.append('file', fileStream, fileName);

        let apiPath = `/api/batch-adj-balance/${brandId}`;
        if (!clientToken || clientToken.length > 0)
            apiPath += `/${clientToken}`;

        return this.http
            .post(
                apiPath,
                formData,
                {
                    headers: {
                        ...formData.getHeaders(),
                        "Content-Length": formData.getLengthSync()
                    }
                })
            .then(res => res.data);
    }

    public previewBatchAdjustBatchData(batchId: string): Promise<BatchAdjustBalanceItem[]> {
        return this.http
            .get(`/api/batch-adj-balance/${batchId}`)
            .then(res => res.data);
    }

    public processAdjustBatch(
        brandId: string,
        batchId: string,
        condition: BatchAdjustBalanceProcessCondition): Promise<number> {

        return this.http
            .put(`/api/batch-adj-balance/${brandId}/${batchId}`, condition)
            .then(res => res.status);
    }

    public checkAdjustBatchIsCompleted(batchId: string): Promise<number> {
        return this.http
            .head(`/api/batch-adj-balance/${batchId}`)
            .then(res => res.status);
    }

    public getBatchAdjustProcessResult(batchId: string): Promise<BatchAdjustBalanceItemResult[]> {
        return this.http
            .get(`/api/batch-adj-balance/result/${batchId}`)
            .then(res => res.data);
    }

    public createOfflineLoginRewardPromotion(
        brandId: string,
        request: createOfflineLoginRewardRequest): Promise<MessageResponse> {
        return this.http
            .put(`/api/offline-login-reward/${brandId}:create`, request)
            .then(res => res.data);
    }

    public checkOfflineBackgroundProcessIsCompleted(batchId: string): Promise<number> {
        return this.http
            .head(`/api/offline/${batchId}`)
            .then(res => res.status);
    }

    public downloadOfflinePromotionFormTemplate(): Promise<{ status: number; contentType: string }> {
        return this.http
            .get('/api/offline/template')
            .then(res => {
                let contentType: string;

                if (typeof res.headers.getContentType === 'string' || typeof res.headers.getContentType === 'undefined')
                    contentType = res.headers.getContentType ?? '';
                else
                    contentType = res.headers.getContentType()?.toString() ?? '';

                return {
                    status: res.status,
                    contentType: contentType
                };
            });
    }

    public uploadOfflineUserList(
        brandId: string,
        filePath: string): Promise<uploadOfflineLoginRewardUserListResponse> {

        let stream = fs.readFileSync(filePath);

        let formData = new FormData();
        formData.append("file", stream, 'file.xlsx');

        let config = {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        };

        return this.http
            .post(`/api/offline/${brandId}`, formData, {
                headers: {
                    ...formData.getHeaders(),
                    "Content-Length": formData.getLengthSync()
                },
            })
            .then(res => res.data);
    }

    public createOfflineDepositRewardPromotion(
        brandId: string,
        request: CreateOfflineDepositRewardPromotionRequest): Promise<MessageResponse> {
        return this.http
            .put(`/api/offline-deposit-reward/${brandId}:create`, request)
            .then(res => res.data);
    }

    public updatePromotionContent(
        promotionId: string,
        request: UpdatePromotionContentRequest): Promise<MessageResponse> {
        return this.http
            .put(`/api/promotion/${promotionId}:content`, request)
            .then(res => res.data);
    }
}