import { AxiosInstance } from "axios";

export default class BackOfficeService {
    constructor(private http: AxiosInstance) { }

    public adjustPlayerLockedAmount(playerCode: string, lockAmount: number, remark: string | null): Promise<void> {
        return this.http.put(`/api/player/wallet/locked/${playerCode}`, { lockAmount, remark });
    }

    public getPlayerIpsByUserName(brandId: string, userName: string, page: number, rows: number, sortby: string, orderby: string, showAll: boolean) {
        return this
            .http
            .get(`/api/playerip/username`,
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
                });
    }
}