interface DraftFormListItem {
    id: string;
    description: string;
    publishDate?: string;
    modifier: string;
    status: ArticleStatus;
    createdOn: Date;
    updatedOn: Date;
}
