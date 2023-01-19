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
}