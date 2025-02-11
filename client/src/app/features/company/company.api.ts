import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { axiosBaseQuery } from "../../../api/axios.baseQuery";
import { environment } from "../../../environment/environment";
import { CompanyCreate } from "../../../models/common/company.create";
import { Company } from "../../../models/common/company.models";


export const companyApi = createApi({
    reducerPath: 'companyApi',
    baseQuery: axiosBaseQuery({ baseUrl: environment.apiUrl }),
    tagTypes: ['Company'],
    keepUnusedDataFor: 5,
    endpoints: (builder) => ({
        getProfileCompanies: builder.query<Company[], void>({
            query: () => ({
                url: `/company`,
                method: 'get'
            }),
            providesTags: ['Company']
        }),
        updateCompany: builder.mutation<Company, CompanyCreate>({
            query: (company: CompanyCreate) => ({
                url: `/company`,
                method: 'put',
                data: company
            }),
            invalidatesTags: ['Company']
        }),
        createCompany: builder.mutation<Company, Company>({
            query: (company: Company) => ({
                url: `/company`,
                method: 'post',
                data: company
            }),
            invalidatesTags: ['Company']
        }),
    }),
})


export const { useLazyGetProfileCompaniesQuery, useUpdateCompanyMutation, useCreateCompanyMutation } = companyApi;