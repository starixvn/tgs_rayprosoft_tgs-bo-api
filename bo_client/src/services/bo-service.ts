import { AxiosInstance } from "axios";
import fs from 'fs';
import FormData from 'form-data';
import path from 'path';
import { promises } from "dns";

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

    public updatePlayer(playerCode: string, type: string, request: UpdatePlayerRequest): Promise<MessageResponse> {
        return this.http
            .put(`/api/player/${playerCode}/${type}`, request)
            .then(res => res.data);
    }

    public getMarketAgentProfitsReport(request: GetMarketAgentProfitsReportRequest): Promise<PagedResponse<GetMarketAgentProfitsReportModel>> {
        return this.http
            .get(
                `/api/marketreport/agentprofit`, {
                params: {
                    ...request
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
        filePath: string): Promise<UploadOfflineLoginRewardUserListResponse> {

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
            .patch(`/api/promotion/content/${promotionId}`, request)
            .then(res => res.data);
    }

    public updatePromotionRule(
        promotionId: string,
        request: UpdatePromotionRuleRequest): Promise<MessageResponse> {
        return this.http
            .patch(`/api/promotion/rule/${promotionId}`, request)
            .then(res => res.data);
    }

    public updatePromotionStatus(
        promotionId: string,
        status: string): Promise<MessageResponse> {
        return this.http
            .put(`/api/promotion/${promotionId}/${status}`)
            .then(res => res.data);
    }

    public updatePromotionMainInfo(
        promotionId: string,
        request: UpdatePromotionMainInfoRequest): Promise<MessageResponse> {
        return this.http
            .patch(`/api/promotion/main/${promotionId}`, request)
            .then(res => res.data);
    }

    public createPromotionMainInfo(
        brandId: string,
        request: CreatePromotionMainInfoRequest): Promise<MessageResponse> {
        return this.http
            .put(`/api/promotion/main/${brandId}`, request)
            .then(res => res.data);
    }

    public createPromotion(
        request: CreatePromotionRequest): Promise<MessageResponse> {
        return this.http
            .post('/api/promotion', request)
            .then(res => res.data);
    }

    public updatePromotion(
        promotionId: string,
        request: UpdatePromotionRequest): Promise<MessageResponse> {
        return this.http
            .put(`/api/promotion/${promotionId}`, request)
            .then(res => res.data);
    }

    public getPromotionRule(
        promotionId: string): Promise<string> {
        return this.http
            .get(`/api/promotion/rule/${promotionId}`)
            .then(res => res.data);
    }

    public getPromotionContent(
        promotionId: string): Promise<PromtionContent> {
        return this.http
            .get(`/api/promotion/content/${promotionId}`)
            .then(res => res.data);
    }

    public getPromotionMainInfo(
        promotionId: string): Promise<PromotionMainInfo> {
        return this.http
            .get(`/api/promotion/main/${promotionId}`)
            .then(res => res.data);
    }

    public duplicatePromotion(
        basePromotionId: string): Promise<ApiResponse<string>> {
        return this.http
            .put(`/api/promotion/${basePromotionId}:duplicate`)
            .then(res => res.data);
    }

    public getBrandList(
        request: GetBrandsRequest,
        paging: PageableRequest): Promise<PagedResponse<BrandListItem>> {
        return this.http
            .get('/api/brand',
                {
                    params: {
                        ...request,
                        ...paging
                    }
                }
            )
            .then(res => res.data);
    }

    public createBrand(
        request: CreateBrandRequest): Promise<MessageResponse> {
        return this.http
            .post('/api/brand', request)
            .then(res => res.data);
    }

    public updateBrandStatus(
        brandId: string,
        status: string): Promise<MessageResponse> {
        return this.http
            .put(`/api/brand/${brandId}/${status}`)
            .then(res => res.data);
    }

    public setAllBrandsMaintenance(): Promise<MessageResponse> {
        return this.http
            .put('/api/brandmaintenance/allmaintain')
            .then(res => res.data);
    }

    public getPromotionList(
        request: GetPromotionsRequest): Promise<PagedResponse<PromotionModel>> {
        return this.http
            .get('/api/promotion',
                {
                    params: {
                        ...request
                    }
                }
            )
            .then(res => res.data);
    }

    public getBrandLetters(
        brandId: string,
        playerCode: string | null,
        page: number,
        rows: number
    ): Promise<PagedResponse<StationLetterListItem>> {
        return this.http
            .get(
                `/api/station-letter/${brandId}`,
                {
                    params: {
                        player: playerCode,
                        page,
                        rows
                    }
                })
            .then(res => res.data);
    }

    public async sendBrandLetter(brandId: string, request: CreateStationLetterRequest): Promise<void> {
        await this.http
            .put(`/api/station-letter/player/${brandId}`, request);
    }

    public async sendAllPlayersBrandLetter(brandId: string, request: CreateAllPlayersStationLetterRequest): Promise<void> {
        await this.http
            .put(`/api/station-letter/all/${brandId}`, request);
    }

    public async replyBrandLetter(brandId: string, letterId: string, content: string): Promise<void> {
        await this.http
            .put(`/api/station-letter/${brandId}/${letterId}:reply`, { content });
    }

    public getBrandLetter(brandId: string, letterId: string): Promise<StationLetter | null> {
        return this.http
            .get(`/api/station-letter/${brandId}/${letterId}`)
            .then(res => res.data);
    }

    public getDeliveryLetters(brandId: string, page: number, rows: number): Promise<PagedResponse<DeliveryLetterListItem>> {
        return this.http
            .get(
                `/api/delivery-box/${brandId}`,
                {
                    params: {
                        page,
                        rows
                    }
                })
            .then(res => res.data);
    }

    public getDeliveryLetter(brandId: string, deliveryId: string): Promise<DeliveryLetter | null> {
        return this.http
            .get(`/api/delivery-box/${brandId}/${deliveryId}`)
            .then(res => res.data);
    }

    public queryAdjustSummaries(resellerId: string, from: Date, to: Date): Promise<Iterable<AdjustSummaryListItem>> {
        return this.http
            .get(
                `/api/adjust-wallet/summary/${resellerId}`,
                {
                    params: {
                        from,
                        to
                    }
                })
            .then(res => res.data);
    }

    public queryPlayerAdjustRecords(
        brandId: string,
        condition: AdjustRecordQueryRequest,
        page: number,
        rows: number): Promise<PagedResponse<Iterable<AdjustRecordListItem>>> {
        return this.http
            .get(
                `/api/adjust-wallet/player/${brandId}`,
                {
                    params: {
                        ...condition,
                        page,
                        rows
                    }
                })
            .then(res => res.data);
    }

    public queryAgentAdjustRecords(
        brandId: string,
        condition: AdjustRecordQueryRequest,
        page: number,
        rows: number): Promise<PagedResponse<Iterable<AdjustRecordListItem>>> {
        return this.http
            .get(
                `/api/adjust-wallet/agent/${brandId}`,
                {
                    params: {
                        ...condition,
                        page,
                        rows
                    }
                })
            .then(res => res.data);
    }

    public exportPlayerAdjustRecords(
        brandId: string,
        from: Date,
        to: Date
    ): Promise<string | undefined> {
        return this.http
            .get(
                `/api/adjust-wallet/player/${brandId}:export`,
                {
                    params: {
                        from,
                        to
                    }
                })
            .then<string | undefined>(res => res.headers["content-type"]);
    }

    public exportAgentAdjustRecords(
        brandId: string,
        from: Date,
        to: Date
    ): Promise<string | undefined> {
        return this.http
            .get(
                `/api/adjust-wallet/agent/${brandId}:export`,
                {
                    params: {
                        from,
                        to
                    }
                })
            .then(res => res.headers["content-type"]);
    }
}