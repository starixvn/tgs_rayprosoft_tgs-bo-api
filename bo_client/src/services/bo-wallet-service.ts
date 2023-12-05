import { AxiosInstance } from "axios";

export default class BoWalletService {
    constructor(private http: AxiosInstance) { }

    public getPlayerMainWalletBalance(playerCode: string): Promise<number> {
        return this.http
            .get(`/api/wallet/player/${playerCode} `)
            .then(res => res.data);
    }

    public getAccountChangesOld(
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

    public getAccountChanges(
        playerCode: string,
        request: PlayerAccountChangesQueryRequest,
        page: number,
        rows: number
    ): Promise<PagedResponse<PlayerAccountChange>> {
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

    public getAccountChangesTotalAmountOld(request: GetPlayerAccountChangesReportRequest)
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

    public getAccountChangesTotalAmount(
        playerCode: string,
        request: PlayerAccountChangesQueryRequest
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
}