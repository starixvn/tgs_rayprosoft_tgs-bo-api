interface BrandListItem {
    id: string;
    code: string;
    name: string;
    licenseeId: string;
    licenseeName: string;
    resellerId: string;
    resellerName: string;
    status: string;
    description?: string;
    operationalEmail?: string;
    createdOn: Date;
    updatedOn: Date;
}