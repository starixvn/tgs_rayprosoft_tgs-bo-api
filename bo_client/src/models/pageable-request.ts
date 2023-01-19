interface PageableRequest {
    orderBy: "Ascending" | "Descending";
    page: number;
    rows: number;
    showAll: boolean;
    sortBy: string;
}