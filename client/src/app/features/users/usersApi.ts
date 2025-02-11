import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { axiosBaseQuery } from "../../../api/axios.baseQuery";
import { environment } from "../../../environment/environment";
import { UserDto } from "../../../models/users/user.dto";
import { UserUpdate } from "../../../models/users/user.update.dto";

export const usersApi = createApi({
    reducerPath: 'usersApi',
    baseQuery: axiosBaseQuery({ baseUrl: environment.apiUrl}),
    tagTypes: ['Users'],
    endpoints: (builder) => ({
        getUsers: builder.query<{items: UserDto[], totalCount: number}, {page: number, pageSize: number}>({
            query: ({page, pageSize}) => ({
                url: `/users`,
                method: 'get',
                params: {
                    page,
                    pageSize
                }
            }),
            providesTags: ['Users']
        }),
        getUser: builder.query<UserDto, string>({
            query: (id: string) => ({
                url: `/user/${id}`,
                method: 'get'
            }),
            providesTags: ['Users']
        }),
        updateUser: builder.mutation<UserDto, UserUpdate>({
            query: (body: UserUpdate) => ({
                url: `/user`,
                method: 'put',
                data: body
            }),
            invalidatesTags: ['Users']
        }),
        deleteUser: builder.mutation<void, string>({
            query: (id: string) => ({
                url: `/user/${id}`,
                method: 'delete'
            }),
            invalidatesTags: ['Users']
        })
    })
})

export const { useGetUsersQuery, useGetUserQuery, useDeleteUserMutation, useUpdateUserMutation } = usersApi;