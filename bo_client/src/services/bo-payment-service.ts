import { AxiosInstance } from "axios";

export default class BoPaymentService {
    constructor(private http: AxiosInstance) { }

    public createPlayerWithdraw(request: CreateWithdrawRequest): Promise<MessageResponse> {
        return this.http
            .post("/api/withdraw/player", request)
            .then(res => res.data);
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

    public reviewWithdrawForm(
        formId: string,
        reviewResponse: WithdrawReviewRequest
    ): Promise<void> {
        return this.http
            .post(`/api/withdraw/${formId}`, reviewResponse)
            .then(res => res.data);
    }

    public getWithdrawFormDetail(
        brandId: string,
        formId: string
    ): Promise<GenericResponse<WithdrawFormDetailViewModel>>{
        return this.http
            .get(`/api/withdraw/${brandId}/${formId}`)
            .then(res => res.data);
    }
}