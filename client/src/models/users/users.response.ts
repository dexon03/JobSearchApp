import { UserDto } from "./user.dto";

export type UsersResponse = {
    items: UserDto[];
    totalCount: number;
}