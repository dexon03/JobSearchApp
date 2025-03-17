import { createApi } from '@reduxjs/toolkit/query/react';
import { axiosBaseQuery } from '../../../api/axios.baseQuery';
import { environment } from '../../../environment/environment';
import { LocationDto } from '../../../models/common/location.dto';
import { SkillDto } from '../../../models/common/skill.dto';
import { VacancyFilter } from '../../../models/common/vacancy.filters';
import { StatisticNode } from '../../../models/statistic/statistic.node';
import { Category } from '../../../models/vacancy/category.model';
import { GenerateVacancyDescription } from '../../../models/vacancy/generated.desription';
import { GenerateVacancyDescriptionRequest } from '../../../models/vacancy/generateVacancyDescription.model';
import { VacancyCreate } from '../../../models/vacancy/vacancy.create.dto';
import { VacancyGetAll } from '../../../models/vacancy/vacancy.getall.dto';
import { VacancyUpdateModel } from '../../../models/vacancy/vacancy.update.dto';
import { VacancyGet } from '../../../models/vacancy/vacany.get.dto';

export const vacancyApi = createApi({
    reducerPath: 'vacancyApi',
    tagTypes: ['VacancyAll', 'RecruiterVacancy', 'Statistic'],
    baseQuery: axiosBaseQuery({ baseUrl: environment.apiUrl }),
    keepUnusedDataFor: 5,
    endpoints: (builder) => ({
        getVacancies: builder.query<VacancyGetAll[], VacancyFilter>({
            query: (filter: VacancyFilter) => ({
                url: '/vacancy',
                method: 'get',
                params: filter
            }),
            providesTags: ['VacancyAll']
        }),
        getRecruiterVacancies: builder.query<VacancyGetAll[], { recruiterId: string, filter: VacancyFilter }>({
            query: ({ recruiterId, filter }) => ({
                url: '/vacancy/recruiterVacancies/' + recruiterId,
                method: 'get',
                params: filter
            }),
            providesTags: ['RecruiterVacancy']
        }),
        getVacancy: builder.query<VacancyGet, string>({ query: (id: string) => ({ url: `/vacancy/${id}`, method: 'get' }) }),
        createVacancy: builder.mutation<VacancyGet, VacancyCreate>({
            query: (body: VacancyCreate) => ({
                url: '/vacancy',
                method: 'post',
                data: body
            }),
            invalidatesTags: ['VacancyAll', 'RecruiterVacancy', 'Statistic']
        }),
        getVacancyLocation: builder.query<LocationDto[], void>({ query: () => ({ url: '/location', method: 'get' }) }),
        getVacancySkills: builder.query<SkillDto[], void>({ query: () => ({ url: `/skill`, method: 'get' }) }),
        getVacancyCategories: builder.query<Category[], void>({ query: () => ({ url: `/category`, method: 'get' }) }),
        activateDisactivateVacancy: builder.mutation<void, string>({
            query: (id: string) => ({
                url: `/vacancy/${id}/activate-deactivate`,
                method: 'put'
            }),
            invalidatesTags: ['VacancyAll', 'RecruiterVacancy', 'Statistic']
        }),
        updateVacancy: builder.mutation<VacancyGet, VacancyUpdateModel>({
            query: (body: VacancyUpdateModel) => ({
                url: `/vacancy`,
                method: 'put',
                data: body
            }),
            invalidatesTags: ['VacancyAll', 'RecruiterVacancy', 'Statistic']
        }),
        deleteVacancy: builder.mutation<void, string>({
            query: (id: string) => ({
                url: `/vacancy/${id}`,
                method: 'delete'
            }),
            invalidatesTags: ['VacancyAll', 'RecruiterVacancy', 'Statistic']
        }),
        getStatistic: builder.query<StatisticNode[], string | null>({
            query: (skillName?: string | null) => ({
                url: '/statistic' + '?skillName=' + skillName,
                method: 'get'
            }),
            providesTags: ['Statistic']
        }),
        getMockedStatistic: builder.query<StatisticNode[], void>({
            query: () => ({
                url: '/statistic/mocked',
                method: 'get'
            }),
            providesTags: ['Statistic']
        }),
        generateVacancyDesciprtion: builder.query<GenerateVacancyDescription, GenerateVacancyDescriptionRequest>({
            query: (request: GenerateVacancyDescriptionRequest) => ({
                url: '/vacancy/getDescription',
                method: 'post',
                data: request,
                timeout: 120000,
            }),
            extraOptions: {
                maxRetries: 3,
                retryCondition: (err: any) => {
                    return err.status === 504 || err.status === 408;
                },
                backoff: (attempt: number) => Math.min(1000 * (2 ** attempt), 30000),
            },
        }),
    }),
});

export const {
    useGetVacanciesQuery,
    useGetRecruiterVacanciesQuery,
    useGetVacancyQuery,
    useCreateVacancyMutation,
    useGetVacancySkillsQuery,
    useLazyGetVacancyLocationQuery,
    useLazyGetVacancySkillsQuery,
    useLazyGetVacancyCategoriesQuery,
    useActivateDisactivateVacancyMutation,
    useUpdateVacancyMutation,
    useDeleteVacancyMutation,
    useLazyGetStatisticQuery,
    useGetMockedStatisticQuery,
    useLazyGenerateVacancyDesciprtionQuery,
} = vacancyApi;

export const { useQuerySubscription: useQuerySubscriptionGetAllVacancies } = vacancyApi.endpoints.getVacancies;