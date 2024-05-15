import { AxiosHeaders, AxiosInstance } from "axios";

export default class BoBrandService {
    // #region Constructors (1)

    constructor(private http: AxiosInstance) { }

    // #endregion Constructors (1)

    // #region Public Methods (10)

    public createBrand(
        request: CreateBrandRequest): Promise<MessageResponse> {
        return this.http
            .post('/api/brand', request)
            .then(res => res.data);
    }

    public getBrandAutoWithdrawSetting(brandId: string): Promise<AutoWithdrawInfo> {
        return this.http
            .get(`api/brandpayment/autowithdraw/${brandId}`)
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

    public getBrandMaintenanceInfos(
        request: GetBrandMaintenanceInfosRequest
    ): Promise<BrandMaintenanceInfo[]> {
        return this.http
            .get(`/api/BrandMaintenance`,
                {
                    params: {
                        ...request,
                    }
                })
            .then(res => res.data);
    }

    public getBrandPlatformAccountFundFlag(brandId: string): Promise<boolean> {
        return this.http
            .get(`/api/brand/platform-account-fund/${brandId}`)
            .then(res => res.data);
    }

    public setAllBrandsMaintenance(): Promise<MessageResponse> {
        return this.http
            .put('/api/brandmaintenance/allmaintain')
            .then(res => res.data);
    }

    public setBrandMaintenanceTime(
        brandId: string,
        request: UpdateBrandMaintenanceTimeRequest): Promise<MessageResponse> {
        return this.http
            .put(`/api/BrandMaintenance/time/${brandId}`,
                {
                    ...request
                })
            .then(res => res.data);
    }

    public setBrandPlatformAccountFundFlag(brandId: string, enabled: boolean): Promise<void> {
        let headers = new AxiosHeaders();
        headers.setContentType("application/json");

        return this.http
            .patch(`/api/brand/platform-account-fund/${brandId}`, enabled, { headers })
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
            .patch(`/api/brandpayment/autowithdraw/${brandId}`,
                {
                    ...request
                })
            .then(res => res.data);
    }

    public listBrandSocialMediaSettings(brandId: string): Promise<SocialMediaSetting[]> {
        return this.http
            .get(`/api/brand/social/${brandId}`)
            .then(res => res.data);
    }

    public addBrandSocialMediaSetting(brandId: string, setting: AddSocialMediaSetting): Promise<string> {
        return this.http
            .put(`/api/brand/social/${brandId}`, setting)
            .then(res => res.data);
    }

    public setSocialMediaSettingOrder(settingId: string, newOrder: number): Promise<void> {
        let headers = new AxiosHeaders();
        headers.setContentType("application/json");

        return this.http
            .patch(`/api/brand/social/order/${settingId}`, newOrder, { headers })
            .then(res => res.data);
    }
    // #endregion Public Methods (10)
}