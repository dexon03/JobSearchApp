import { BaseQueryFn } from "@reduxjs/toolkit/query"
import type { AxiosRequestConfig, AxiosError } from 'axios'
import api from "./api"

const cleanParams = (params: AxiosRequestConfig['params']) => {
    if (!params) return undefined;
    return Object.fromEntries(
        Object.entries(params).filter(([, value]) =>
            value !== null &&
            value !== undefined &&
            value !== '' &&
            !(Array.isArray(value) && value.length === 0)
        )
    );
};

export const axiosBaseQuery = ({ baseUrl }: { baseUrl: string } = { baseUrl: '' }): BaseQueryFn<
    {
        url: string
        method: AxiosRequestConfig['method']
        data?: AxiosRequestConfig['data']
        params?: AxiosRequestConfig['params']
        headers?: AxiosRequestConfig['headers']
    },
    unknown,
    unknown
> =>
    async ({ url, method, data, params, headers }) => {
        try {
            const result = await api({
                url: baseUrl + url,
                method,
                timeout: 10000000,
                data,
                params: cleanParams(params),
                headers,
            })
            return { data: result.data }
        } catch (axiosError) {
            const err = axiosError as AxiosError
            return {
                error: {
                    status: err.response?.status,
                    data: err.response?.data || err.message,
                },
            }
        }
    }