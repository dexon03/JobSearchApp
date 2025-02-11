import { useNavigate, useParams } from "react-router-dom";
import { useGetUserQuery, useUpdateUserMutation } from "../../app/features/users/usersApi";
import { useEffect, useState } from "react";
import { Button, Container, TextField } from "@mui/material";
import { Role } from "../../models/common/role.enum";

export function UserEdit() {
    const { id } = useParams();
    const { data: user, isLoading } = useGetUserQuery(id);
    const [updateUser, { isError }] = useUpdateUserMutation();
    const navigate = useNavigate();

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [role, setRole] = useState(0);

    useEffect(() => {
        if (user) {
            setFirstName(user.firstName);
            setLastName(user.lastName);
            setEmail(user.email);
            setPhoneNumber(user.phoneNumber);
            setRole(user.role.name == 'Candidate' ? 0 : 1);
        }
    }, [user])


    if (isLoading) {
        return <div>Loading...</div>
    }

    const handleSubmit = async (e) => {
        e.preventDefault();
        var result = await updateUser({
            id: user.id,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: phoneNumber,
            role: role
        });
        if (!isError) {
            navigate('/users');
        }
    }

    return (
        user &&
        <Container component="main" maxWidth="sm">
            <h2>Edit User</h2>
            <form>
                <TextField
                    label="First Name"
                    name="firstName"
                    value={firstName}
                    required
                    onChange={(e) => setFirstName(e.target.value)}
                    fullWidth
                    margin="normal"
                />
                <TextField
                    label="Last Name"
                    name="lastName"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    fullWidth
                    margin="normal"
                />
                <TextField
                    label="Email"
                    name="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    fullWidth
                    margin="normal"
                />
                <TextField
                    label="Phone Number"
                    name="phoneNumber"
                    value={phoneNumber}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                    fullWidth
                    margin="normal"
                />
                <TextField
                    label="Role Name"
                    name="roleName"
                    value={Role[role]}
                    fullWidth
                    disabled
                    margin="normal"
                />
                <Button variant="contained" color="primary" onClick={handleSubmit}>
                    Save Changes
                </Button>
            </form>
        </Container>
    )
}