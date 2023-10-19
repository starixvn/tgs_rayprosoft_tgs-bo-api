interface CreateNewDraftForm {
    description: string;
    publishDate?: string;
    localizedArticles: { [key: string]: Article };
}