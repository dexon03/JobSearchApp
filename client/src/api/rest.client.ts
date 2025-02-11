import { environment } from "../environment/environment.ts";
import api from "./api.ts";

export class RestClient {
    private readonly apiUrl: string = environment.apiUrl;
    
    async get<T>(url: string): Promise<T> {
        const response = await api.get(this.apiUrl + url);
        if (response.status >= 400) {
            throw new Error(response.data);
        }
        return response.data;
    }

    async post<T>(url: string, body: unknown): Promise<T> {
        const response = await api.post(this.apiUrl + url, body);
        if (response.status >= 400) {
            throw new Error(await response.data);
        }
        return response.data;
    }

    async put<T>(url: string, body: unknown): Promise<T> {
        const response = await api.put(this.apiUrl + url, body);
        if (response.status >= 400) {
            throw new Error(await response.data);
        }
        return response.data;
    }

    async delete<T>(url: string): Promise<T> {
        const response = await api.delete(this.apiUrl + url);
        if (response.status >= 400) {
            throw new Error(await response.data);
        }
        return response.data;
    }
}