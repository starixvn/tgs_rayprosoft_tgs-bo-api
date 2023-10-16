import * as qs from 'qs';

import { AxiosInstance } from "axios";
import FormData from 'form-data';
import fs from 'fs';
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
    ): Promise<PagedResponse<PlayersModel>> {
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
        let queryStrings = {
            ...request,
            promotionType: request.promotionTypes
        };

        delete queryStrings.promotionTypes;

        return this.http
            .get(
                '/api/promotion',
                {
                    params: queryStrings,
                    paramsSerializer: {
                        serialize: params => {
                            return qs.stringify(params, { arrayFormat: "repeat" });
                        }
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

    public getDeliveryLetters(
        brandId: string,
        operator: string | null,
        page: number,
        rows: number
    ): Promise<PagedResponse<DeliveryLetterListItem>> {
        return this.http
            .get(
                `/api/delivery-box/${brandId}`,
                {
                    params: {
                        operator,
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

    public exportAllAdjustRecords(
        resellerId: string,
        from: Date,
        to: Date
    ): Promise<string | undefined> {
        return this.http
            .get(
                `/api/adjust-wallet/summary/${resellerId}:export`,
                {
                    params: {
                        from,
                        to
                    }
                })
            .then(res => res.headers["content-type"]);
    }

    public listPlayerCodesByUserNamePrefix(
        brandId: string,
        userNamePrefix: string
    ): Promise<Iterable<{}>> {
        return this.http
            .get(`/api/player/${brandId}/${userNamePrefix}`)
            .then(res => res.data);
    }

    public getSinglePlayer(
        playerCode: string): Promise<GetPlayerResponse> {
        return this.http
            .get(`/api/player/${playerCode}`)
            .then(res => res.data);
    }

    public async updatePlayerIdentityAuthentication(
        playerCode: string,
        identityAuthentication: boolean
    ): Promise<void> {
        await this.http
            .put(`/api/player/identityauth/${playerCode}/${identityAuthentication}`);
    }

    public getAccountChanges(
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

    public getAccountChangesTotalAmount(request: GetPlayerAccountChangesReportTotalAmountRequest)
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

    public createAgentReduceAmount(
        id: string,
        request: CreateAgentReduceAmountRequest): Promise<MessageResponse> {
        return this.http
            .post(`api/Agent/${id}/adjust/reduce`,
                {
                    params: {
                        ...request
                    }
                })
            .then(res => res.data);
    }

    public getPlayerBucketSummaryAmount(
        playerCode: string
    ): Promise<number> {
        return this.http
            .get(`/api/player/bucket/amount/${playerCode}`)
            .then(res => res.data);
    }

    public getPlayerTypeBucketAmounts(playerCode: string): Promise<Iterable<TypeBucketResult>> {
        return this.http
            .get(`/api/player/bucket/type/amount/${playerCode}`)
            .then(res => res.data);
    }

    public getBrandAutoWithdrawSetting(brandId: string): Promise<AutoWithdrawInfo> {
        return this.http
            .get(`api/brandpayment/autowithdraw/${brandId}`)
            .then(res => res.data);
    }

    public upsertBrandAutoWithdraw(
        brandId: string,
        request: UpsertBrandAutoWithdrawRequest
    ): Promise<MessageResponse> {
        return this.http
            .patch(`api/brandpayment/autowithdraw/${brandId}`,
                {
                    params: {
                        ...request
                    }
                })
            .then(res => res.data);
    }

    public getPlayerBuckets(playerCode: string, bucketType?: string): Promise<Iterable<PlayerBucket>> {
        return this.http
            .get(bucketType == null
                ? `/api/player/bucket/${playerCode}`
                : `/api/player/bucket/${playerCode}/${bucketType}`)
            .then(res => res.data);
    }

    public exportDeposit(brandId: string, request: DepositFormQuery, paging: PageableRequest)
        : Promise<string | undefined> {
        const queryStringMap: { [name: string]: string } = {
            "paymentId": "payment",
            "paymentMethod": "method",
            "beginCreatedOn": "bc",
            "endCreatedOn": "ec",
            "beginUpdatedOn": "bu",
            "endUpdatedOn": "eu"
        };

        let queryString: { [name: string]: string } = {};
        const query: any = request;

        for (var propName in Object.keys(request)) {
            var queryStringKey = queryStringMap[propName] ?? propName;
            queryString[queryStringKey] = query[propName];
        }

        Object.assign(queryString, paging);

        return this.http
            .get(
                `/api/deposit/exportDeposit/${brandId}`,
                {
                    params: queryString
                })
            .then(res => res.headers["content-type"]);
    }

    public exportWithdraw(brandId: string, request: WithdrawFormQuery, paging: PageableRequest)
        : Promise<string | undefined> {
        const queryStringMap: { [name: string]: string } = {
            "paymentId": "payment",
            "paymentMethod": "method",
            "beginCreatedOn": "bc",
            "endCreatedOn": "ec",
            "beginUpdatedOn": "bu",
            "endUpdatedOn": "eu",
            "brandPaymentIds": "bpid",
        };

        let queryString: { [name: string]: string } = {};
        const query: any = request;

        for (var propName in Object.keys(request)) {
            var queryStringKey = queryStringMap[propName] ?? propName;
            queryString[queryStringKey] = query[propName];
        }

        Object.assign(queryString, paging);

        return this.http
            .get(
                `/api/withdraw/exportWithdraw/${brandId}`,
                {
                    params: queryString
                })
            .then(res => res.headers["content-type"]);
    }

    public getDaySuccessRate(brandId: string): Promise<DayDepositSuccessRateViewModel> {
        return this.http
            .get(
                `/api/overview/success-rate/${brandId}`
            )
            .then(res => res.data);
    }

    public getDayNetDeposit(brandId: string): Promise<NetDepositViewModel> {
        return this.http
            .get(
                `/api/overview/net-deposit/${brandId}`
            )
            .then(res => res.data);
    }

    public getDayRegisterPlayersCountAsync(brandId: string): Promise<DayPlayerRegisterCountViewModel> {
        return this.http
            .get(
                `/api/overview/player/reg/count/${brandId}`)
            .then(res => res.data);
    }

    public getCurrentDayLoginPlayersCountAsync(brandId: string): Promise<number> {
        return this.http
            .get(
                `/api/overview/player/login/count/${brandId}`)
            .then(res => res.data);
    }

    public createPlayerWithdraw(request: CreateWithdrawRequest): Promise<MessageResponse> {
        return this.http
            .post("/api/withdraw/player", request)
            .then(res => res.data);
    }

    public createVipLevel(brandId: string, request: ModifyVipLevel) {
        return this.http
            .post("/api/viplevel", {
                brandId,
                ...request
            })
            .then(res => res.data);
    }

    public getVipLevels(brandId: string): Promise<PagedResponse<Viplevel>> {
        return this.http
            .get("/api/viplevel", {
                params: {
                    brandId
                }
            })
            .then(res => res.data);
    }

    public updateVipLevel(brandId: string, id: string, request: ModifyVipLevel): Promise<MessageResponse> {
        return this.http
            .put(`/api/viplevel/${id}`, {
                brandId,
                ...request
            })
            .then(res => res.data);
    }

    public getTopTenPlayers(brandId: string, sortType: string, ascending: boolean): Promise<TopTenPlayerViewModel> {
        const queryString = {
            sortType: sortType,
            ascending: ascending
        };

        return this.http
            .get(`/api/overview/top-ten/${brandId}`,
                {
                    params: queryString
                })
            .then(res => res.data);
    }

    public getTopTenGames(brandId: string, sortType: string, ascending: boolean): Promise<TopTenGameViewModel> {
        const queryString = {
            sortType: sortType,
            ascending: ascending
        };

        return this.http
            .get(`/api/overview/top-ten/game/${brandId}`,
                {
                    params: queryString
                })
            .then(res => res.data);
    }

    public updateAgentPayPassword(request: UpdateAgentPayPasswordRequest): Promise<MessageResponse> {
        return this.http
            .patch("/api/agent/pay-password", {
                ...request
            })
            .then(res => res.data);
    }

    public getAgentOperations(id: string, paging: PageableRequest, request: GetAgentOperationsRequest):Promise<PagedResponse<AgentOperationModel>>{   
        const queryString = {
            brandId: request.brandId,
            from: request.from,
            to: request.to,
            page: paging.page,
            rows: paging.rows,
            showAll: paging.showAll,
            sortBy: paging.sortBy,
            orderBy: paging.orderBy
        };

        return this.http
            .get(`/api/agent/${id}/operations`,{
                params: queryString
            })
        .then(res=>res.data);
    }

    public getPlayerHistories(playerCode:string, request: PlayerHistoriesQueryViewModel, page: number, rows:number): Promise<PagedResponse<PlayerHistoryViewModel>>{
        const queryString = {
            from: request.from,
            to : request.to,
            page:page,
            rows:rows,
        }
        return this.http
            .get(`/api/player/histories/${playerCode}`,{
                params:queryString
            })
            .then(res=>res.data);
    }

    public getPlayerHistory(eventId:string): Promise<PlayerHistoryContentViewModel>{
        return this.http
            .get(`/api/player/history/${eventId}`)
            .then(res=>res.data);
    }
    
    public async setPlayerPassword(playerCode: string, password: string) : Promise<MessageResponse>{
        return this.http
            .patch(`/api/player/password/${playerCode}/${password}`)
            .then(res=>res.data);
    }
}