export type PaginationInfo = {
    pageCount: number;
    totalItemCount: number;
    pageNumber: number;
    pageSize: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
    isFirstPage: boolean;
    isLastPage: boolean;
    firstItemOnPage: number;
    lastItemOnPage: number;
};
export type VacancyPaginated<T> = {
    vacancies: T[];
    metaData: PaginationInfo;
};