import { Role } from "../common/role.enum";

export interface UserUpdate {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber: string;
    role: Role;
}