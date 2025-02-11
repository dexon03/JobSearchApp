import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { axiosBaseQuery } from "../../../api/axios.baseQuery";
import { environment } from "../../../environment/environment";
import { CandidateFilter } from "../../../models/common/candidates.filter";
import { CandidateProfile } from "../../../models/profile/candidate.profile.model";

export const candidateApi = createApi({
    reducerPath: 'candidateApi',
    baseQuery: axiosBaseQuery({ baseUrl: environment.apiUrl + "/api" }),
    keepUnusedDataFor: 5,
    endpoints: (builder) => ({
        getCandidatesProfile: builder.query<CandidateProfile[], CandidateFilter>({
            query: (filter: CandidateFilter) => ({
                url: `/profile/getCandidatesProfile?`,
                method: 'get',
                params: filter
            })
        }),
    })
})


export const { useGetCandidatesProfileQuery } = candidateApi;