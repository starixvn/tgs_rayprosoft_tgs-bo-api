import { AxiosInstance } from "axios";

export default class BoBrandService {
    constructor(private http: AxiosInstance) { }

    public createBrand(
        request: CreateBrandRequest): Promise<MessageResponse> {
        return this.http
            .post('/api/brand', request)
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

    public getBrandAutoWithdrawSetting(brandId: string): Promise<AutoWithdrawInfo> {
        return this.http
            .get(`api/brandpayment/autowithdraw/${brandId}`)
            .then(res => res.data);
    }

    public setAllBrandsMaintenance(): Promise<MessageResponse> {
        return this.http
            .put('/api/brandmaintenance/allmaintain')
            .then(res => res.data);
    }

    public updateBrandStatus(
        brandId: string,
        status: string): Promise<MessageResponse> {
        return this.http
            .put(`/api/brand/${brandId}/${status}`)
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
}