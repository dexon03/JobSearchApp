import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { axiosBaseQuery } from "../../../api/axios.baseQuery";
import { environment } from "../../../environment/environment";
import { ChatCreateDto } from "../../../models/chat/chat.create.dto";
import { ChatDto } from "../../../models/chat/chat.dto";
import { MessageDto } from "../../../models/chat/message.dto";


export const chatApi = createApi({
    reducerPath: 'chatApi',
    baseQuery: axiosBaseQuery({ baseUrl: environment.apiUrl }),
    tagTypes: ['ChatList', 'ChatMessages'],
    endpoints: (builder) => ({
        getChatList: builder.query<{ items: ChatDto[], totalCount: number }, { userId: string, page: number, pageSize: number }>({
            query: ({ userId, page, pageSize }) => ({
                url: `/chat/list`,
                method: 'get',
                params: {
                    userId,
                    page,
                    pageSize
                }
            }),
            providesTags: ['ChatList']
        }),
        getChatMessages: builder.query<MessageDto[], string>({
            query: (chatId: string) => ({
                url: `/chat/messages/` + chatId,
                method: 'get'
            }),
            providesTags: ['ChatMessages']
        }),
        createChat: builder.mutation<void, ChatCreateDto>({
            query: (chat: ChatCreateDto) => ({
                url: `/chat/create`,
                method: 'post',
                data: chat
            }),
            invalidatesTags: ['ChatList', 'ChatMessages']
        }),
    })
})



export const { useGetChatListQuery, useGetChatMessagesQuery, useCreateChatMutation } = chatApi;