import { useState } from "react";
import { Role } from "../models/common/role.enum";

export default function useRole() {

    function saveRole(role: string) {
        const roleName = Role[role as keyof typeof Role];
        localStorage.setItem('role', roleName.toString());
        setRole(roleName);
    }

    function getRole() {
        const tokenStorage = localStorage.getItem('role');
        if (tokenStorage) {
            return Role[tokenStorage as keyof typeof Role];
        }
        return null;
    }
    const [role, setRole] = useState<Role | null>(getRole());

    return {
        setRole: saveRole,
        role: role
    }
}