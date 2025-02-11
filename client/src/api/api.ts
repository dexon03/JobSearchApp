import axios, { AxiosError } from 'axios';
import { showErrorToast } from "../app/features/common/popup";
import { environment } from "../environment/environment";

const api = axios.create({
    baseURL: environment.apiUrl,
});


api.interceptors.request.use(
    (config) => {
        const storageToken = localStorage.getItem('token');
        const token = storageToken ? JSON.parse(storageToken)?.accessToken : null;
        if (token) {
            config.headers!.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        if (error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            try {
                const storageToken = localStorage.getItem('token');
                const refreshToken = storageToken ? JSON.parse(storageToken)?.refreshToken : null;
                const response = await axios.post(environment.apiUrl + '/api/identity/refresh', { refreshToken })

                const token = response.data;
                const stringToken = JSON.stringify(token);

                localStorage.setItem('token', stringToken);

                originalRequest.headers.Authorization = `Bearer ${token.accessToken}`;
                return axios(originalRequest);
            } catch (error: unknown) {
                if (error instanceof AxiosError && error?.response?.status === 401) {
                    localStorage.removeItem('token');
                    window.location.href = '/login';
                }
                console.log(error);
            }
        }
        if (error.response.status === 422) {
            showErrorToast(Object.values(error.response.data).join('\n'));
        }
        if (error.response.status === 500 || error.response.status === 400) {
            showErrorToast(error.response.data.message);
        }

        return Promise.reject(error);
    }
);

export default api;