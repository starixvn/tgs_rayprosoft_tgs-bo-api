import { AxiosInstance } from "axios";

export default class BackOfficeService {
    constructor(private http: AxiosInstance) { }

    public adjustPlayerLockedAmount(playerCode: string, lockAmount: number, remark: string | null): Promise<void> {
        return this.http.put(`/api/player/wallet/locked/${playerCode}`, { lockAmount, remark });
    }
}