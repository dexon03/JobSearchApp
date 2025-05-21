import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { axiosBaseQuery } from "../../../api/axios.baseQuery";
import { environment } from "../../../environment/environment";
import { LocationDto } from "../../../models/common/location.dto";
import { Role } from "../../../models/common/role.enum";
import { SkillDto } from "../../../models/common/skill.dto";
import { CandidateProfile } from "../../../models/profile/candidate.profile.model";
import { RecruiterProfile } from "../../../models/profile/recruiter.profile.model";
import { UpdateRecruiterProfileModel } from "../../../models/profile/updateRecruiterProfileModel";
import { GenerateProfileDescriptionRequest } from "../../../models/profile/generateProfileDescriptionRequest";
import { GenerateProfileDescriptionResponse } from "../../../models/profile/generateProfileDescriptionResponse";
import { vacancyApi } from "../vacancy/vacancy.api";

export const profileApi = createApi({
    reducerPath: 'profileApi',
    baseQuery: axiosBaseQuery({ baseUrl: environment.apiUrl }),
    tagTypes: ['CandidateProfile', 'RecruiterProfile', 'PdfResume'],
    endpoints: (builder) => ({
        getUserCandidateProfile: builder.query<CandidateProfile, void>({
            query: () => ({
                url: `/profile/${Role.Candidate}`,
                method: 'get'
            }),
            providesTags: ['CandidateProfile']
        }),
        getUserRecruiterProfile: builder.query<RecruiterProfile, void>({
            query: () => ({
                url: `/profile/${Role.Recruiter}`,
                method: 'get'
            }),
            providesTags: ['RecruiterProfile']
        }),
        getCandidateProfile: builder.query<CandidateProfile, string>({
            query: (id: string) => ({
                url: `/profile/${Role[Role.Candidate]}/${id}`,
                method: 'get'
            }),
            providesTags: ['CandidateProfile']
        }),
        getRecruiterProfile: builder.query<RecruiterProfile, string>({
            query: (id: string) => ({
                url: `/profile/${Role[Role.Recruiter]}/${id}`,
                method: 'get'
            }),
            providesTags: ['RecruiterProfile']
        }),
        updateCandidateProfile: builder.mutation<CandidateProfile, CandidateProfile>({
            query: (profile: CandidateProfile) => ({
                url: `/profile/${Role[Role.Candidate]}`,
                method: 'put',
                data: profile
            }),
            invalidatesTags: ['CandidateProfile'],
            async onQueryStarted(_, { dispatch, queryFulfilled }) {
                try {
                    await queryFulfilled;
                    dispatch(vacancyApi.util.invalidateTags(['VacancyRecommended']));
                } catch (error) {
                    console.error('Error updating candidate profile:', error);
                }
            }
        }),
        updateRecruiterProfile: builder.mutation<UpdateRecruiterProfileModel, RecruiterProfile>({
            query: (profile: UpdateRecruiterProfileModel) => ({
                url: `/profile/${Role[Role.Recruiter]}`,
                method: 'put',
                data: profile
            }),
            invalidatesTags: ['RecruiterProfile']
        }),
        getProfileLocation: builder.query<LocationDto[], void>({
            query: () => ({
                url: '/location',
                method: 'get'
            }),
        }),
        getProfileSkills: builder.query<SkillDto[], void>({
            query: () => ({
                url: `/skill`,
                method: 'get'
            }),
        }),
        getAiDescription: builder.query<GenerateProfileDescriptionResponse, GenerateProfileDescriptionRequest>({
            query: (request: GenerateProfileDescriptionRequest) => ({
                url: `/profile/AiDescription`,
                method: 'post',
                data: request
            }),
        })
    }),
});

export const
    {
        useGetUserCandidateProfileQuery,
        useGetUserRecruiterProfileQuery,
        useGetCandidateProfileQuery,
        useLazyGetCandidateProfileQuery,
        useLazyGetRecruiterProfileQuery,
        useUpdateCandidateProfileMutation,
        useUpdateRecruiterProfileMutation,
        useLazyGetProfileSkillsQuery,
        useLazyGetProfileLocationQuery,
        useLazyGetAiDescriptionQuery,
    } = profileApi;

export const { useQuerySubscription: useQuerySubscriptionCandidate } = profileApi.endpoints.getUserCandidateProfile;
export const { useQuerySubscription: useQuerySubscriptionRecruiter } = profileApi.endpoints.getUserRecruiterProfile;