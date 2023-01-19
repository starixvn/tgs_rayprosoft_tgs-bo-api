interface ApiResponse<TData> {
    errors: string[];
    message: string;
    valid: boolean;
    data: TData;
}