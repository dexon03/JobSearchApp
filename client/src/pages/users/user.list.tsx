import { TableContainer, Table, TableHead, TableRow, TableCell, TableBody, Button, Pagination } from "@mui/material";
import { useDeleteUserMutation, useGetUsersQuery } from "../../app/features/users/usersApi";
import { useNavigate } from "react-router-dom";
import { useState } from "react";

export function UserList() {
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const { data, isLoading, refetch } = useGetUsersQuery({ page, pageSize });
    const [deleteUser, { isLoading: isDeleting }] = useDeleteUserMutation();
    const navigate = useNavigate();

    const handleEdit = (userId: string) => {
        navigate(`/users/${userId}`);
    };

    const handleDelete = (userId: string) => {
        deleteUser(userId);
        refetch();
    };

    const handlePageChange = (event: React.ChangeEvent<unknown>, value: number) => {
        setPage(value);
    };

    if (isLoading) {
        return <div>Loading...</div>;
    }

    return (
        <>
            <TableContainer>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>First Name</TableCell>
                            <TableCell>Last Name</TableCell>
                            <TableCell>Email</TableCell>
                            <TableCell>Role Name</TableCell>
                            <TableCell align="center">Action</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {data?.items?.map((user) => (
                            <TableRow key={user.id}>
                                <TableCell>{user.firstName}</TableCell>
                                <TableCell>{user.lastName}</TableCell>
                                <TableCell>{user.email}</TableCell>
                                <TableCell>{user.role.name}</TableCell>
                                <TableCell align="center">
                                    <Button variant="contained" color="primary" onClick={() => handleEdit(user.id)}>
                                        Edit
                                    </Button>{' '}
                                    <Button variant="contained" color="error" onClick={() => handleDelete(user.id)}>
                                        Delete
                                    </Button>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            <Pagination 
                count={Math.ceil((data?.totalCount ?? 0) / pageSize)} 
                page={page}
                onChange={handlePageChange}
                sx={{ mt: 2, display: 'flex', justifyContent: 'center' }}
            />
        </>
    );
}


