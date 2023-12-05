import { AxiosInstance } from "axios";

export default class BoWalletService {
    constructor(private http: AxiosInstance) { }

    public getPlayerMainWalletBalance(playerCode: string): Promise<number> {
        return this.http
            .get(`/api/wallet/player/${playerCode} `)
            .then(res => res.data);
    }
}