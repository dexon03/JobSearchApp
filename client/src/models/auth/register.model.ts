import { Role } from "../common/role.enum.ts";

export interface RegisterModel {
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber: string;
    password: string;
    role: Role;
}