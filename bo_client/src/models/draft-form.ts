interface DraftForm {
    id: string;
    brandId: string;
    description: string;
    modifier: string;
    publishDate?: string;
    status: ArticleStatus;
    createdOn: Date;
    updatedOn: Date;
    localizedArticles: { [key: string]: Article };
}